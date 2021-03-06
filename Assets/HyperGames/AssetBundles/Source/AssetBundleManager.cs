﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace HyperGames.AssetBundles {

    public class AssetBundleManager : ILoadOpHandler {

        private BundleLoader                       loader;
        private AssetBundleCache                   cache;
        private ObjectPool<BundleLoadOperation>    loadOps;
        private BundleManagerUpdate                updater;
        private AssetBundleManagerConfig           config;
        private List<Coroutine>                    streams;
        private int                                freeStreams;
        private Dictionary<string, Action<string>> loadHandlers;
        private AssetBundleManifest                manifest;
        private Action<string[]>                   onManifestLoaded = str => { };
        private Action<List<string>>               onLoadBundles = str => { };
        private Func<string, string>               onLoadBundle = str => str;
        private readonly ILog                      log;

        public AssetBundleManager(AssetBundleManagerConfig cfg, GameObject owner, ILog log) {
            Init(cfg, owner);
            this.log = log;
        }

        public AssetBundleManager(AssetBundleManagerConfig cfg, GameObject owner) {
            Init(cfg, owner);
        }

        private void Init(AssetBundleManagerConfig cfg, GameObject owner) {
            config = cfg;
            freeStreams = config.numBundleLoaders;
            
            streams = new List<Coroutine>();
            loadHandlers = new Dictionary<string, Action<string>>();
            
            cache = new AssetBundleCache();
            loader = owner.AddComponent<BundleLoader>();
            
            updater = owner.AddComponent<BundleManagerUpdate>();
            updater.Init(this);

            loadOps = new ObjectPool<BundleLoadOperation>(16, true) {
                OnInstantiate = () => new BundleLoadOperation(this)
            };
            loadOps.Fill();
            // check cfg.bundleTarget and start local server if necessary.
        }

        public void AddVariantsResolver(VariantsResolver resolver) {
            onManifestLoaded = resolver.RegisterBundles;
            onLoadBundles = resolver.RemapVariants;
            onLoadBundle = resolver.RemapVariant;
        }

        public void Update() {
            if (freeStreams == 0) {
                return;
            }
            
            for (int i=0; i<loadOps.numSpawned; ++i) {
                BundleLoadOperation op = loadOps.GetInstance(i);
                
                // This prevents the manager from loading bundles from a single loadop in parallell
                // Should check number of bundles left to load.
                // A LoadOp is not necessarily done just because there are no mor bundles to load.
                //if (op.bundlesLeftToLoad == 0) {
                //    continue;
                //}
                if (!op.canLoadBundle) {
                    continue;
                }

                if (freeStreams == 0) {
                    return;
                }

//                Debug.Log("LoadOp "+i+" has "+op.bundlesLeftToLoad+" bundles left to load");
//                Debug.Log("LoadOp "+i+" is ready");
                
                ITransporter transporter = BundlesHelper.GetTransporter(config);
//                Debug.Log("Got transporter "+transporter.GetType());
//                Debug.Log("Start transporter on stream "+streams.Count);
                string path = BundlesHelper.GetPath(config, BundlesHelper.GetPlatformName());
                streams.Add(loader.StartCoroutine(transporter.Load(op, streams.Count, path)));
//                Debug.Log("Streams: "+streams.Count);
                --freeStreams;
//                Debug.Log("Started loading stream. Streams: "+streams.Count+" free streams: "+freeStreams);
            }
        }

        public AssetBundleLoadStatus LoadMasterManifest() {
            string bundleName = BundlesHelper.GetPlatformName();
            loadHandlers.Add(bundleName, OnMasterManifestLoaded);
            BundleLoadOperation loadOp = AddLoadOp(bundleName);
            return new AssetBundleLoadStatus(loadOp);
        }

        public string[] GetManifestBundles() {
            return manifest == null 
                ? null 
                : manifest.GetAllAssetBundles();
        }
        
        public AssetBundleLoadStatus LoadBundle(string bundleName) {
            List<string> bundleNames = new List<string>();
            bundleNames.AddRange(manifest.GetAllDependencies(bundleName));
            bundleNames.Add(bundleName);

            onLoadBundles(bundleNames);

            BundleLoadOperation loadOp = AddLoadOp(bundleNames);
            return new AssetBundleLoadStatus(loadOp);
        }

        public void OnBundleLoaded(string bundleName, int streamIndex, AssetBundle bundle) {
            Log("[ABM] OnBundleLoaded, bundle: "+bundleName+", stream: "+streamIndex);
            cache.Add(bundleName, bundle);
            ++freeStreams;
//            Debug.Log("Free streams: "+freeStreams);
            loader.StopCoroutine(streams[streamIndex]);
//            Debug.Log("Stopped stream "+streamIndex);

            Action<string> loadHandler;
            if (loadHandlers.TryGetValue(bundleName, out loadHandler)) {
                loadHandler(bundleName);
            }
        }

        public void OnBundleFailed(int streamIndex, int retries) {
            Log("[ABM] OnBundleFailed, stream: "+streamIndex);
            if (retries < 3) {
                //start a new load
            }
            ++freeStreams;
            loader.StopCoroutine(streams[streamIndex]);
        }

        public void OnLoadOpComplete(BundleLoadOperation op) {
            Log("[ABM] OnLoadOpComplete");
            loadOps.Despawn(op);
            if (loadOps.numSpawned > 0) {
                return;
            }
            Log("[ABM] All LoadOps complete");
            updater.Deactivate();
            loader.StopAllCoroutines();
            streams.Clear();
//            Debug.Log("Cleared all streams: "+streams.Count);
        }

        public void OnLoadOpFailed(BundleLoadOperation op) {
            Log("[ABM] OnLoadOpFailed");
            Log("[ABM] error: "+op.error+" msg: "+op.errorMsg);
            loadOps.Despawn(op);
            if (loadOps.numSpawned > 0) {
                return;
            }
            Log("[ABM] All LoadOps complete");
            updater.Deactivate();
            loader.StopAllCoroutines();
            streams.Clear();
        }

        public bool GetAsset<T>(string bundleName, string assetName, out T asset) where T : UnityEngine.Object {
            AssetBundle bundle;
            
            if (!cache.TryGetBundle(onLoadBundle(bundleName), out bundle)) {
                asset = null;
                return false;
            }
            asset = bundle.LoadAsset<T>(assetName);
            return asset != null;
        }

        private BundleLoadOperation AddLoadOp(string bundle) {
            return AddLoadOp(new List<string> { bundle });
        }
        
        private BundleLoadOperation AddLoadOp(List<string> bundles) {
            BundleLoadOperation op;
            loadOps.Spawn(out op);
            op.Init(bundles);

            Log("[ABM] Spawned LoadOp with bundles:");
            
            for (int i = 0; i < bundles.Count; ++i) {
                Log("    " + bundles[i]);
            }
            
            
            updater.Activate();
            return op;
        }

        private void OnMasterManifestLoaded(string bundleName) {
            Log("[ABM] OnMasterManifestLoaded");
            loadHandlers.Remove(bundleName);
            AssetBundle bundle;
            cache.TryGetBundle(bundleName, out bundle);
            manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            Log("Manifest: "+manifest);

            onManifestLoaded(manifest.GetAllAssetBundlesWithVariant());
        }

        private void Log(string msg) {
            if (log == null) {
                return;
            }
            log.Log(msg);
        }
    }

}
