using UnityEngine;
using UnityEngine.UI;

public class WaitForButtonClick : CustomYieldInstruction {
    
    private          bool     clicked;
    private readonly Button   btn;
    
    public override bool keepWaiting {
        get { return !clicked; }
    }

    public WaitForButtonClick(Button button) {
        clicked = false;
        btn = button;
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick() {
        clicked = true;
        btn.onClick.RemoveListener(OnClick);
    }
}

