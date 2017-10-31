using System.Text;

namespace HyperGames {
    
    public static class StringUtils {
    
        public static void ClearStringBuilder(StringBuilder sb) {
            sb.Remove(0, sb.Length);
        }
    }

}
