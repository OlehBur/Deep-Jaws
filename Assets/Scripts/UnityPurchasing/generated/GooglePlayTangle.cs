// WARNING: Do not modify! Generated file.
#pragma warning disable UDR0001

namespace UnityEngine.Purchasing.Security {

    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("JNSwrleBXPArtAE51Wjcg6zB+YiUbHLu9lUt93fVOmHHC0Z2M/W1aJSzyWYVsdZjFkET2p4zr2lpryvbRd7Xj69wPWDuRLbG7jo3UKXkcTyfHBIdLZ8cFx+fHBwd+1lBP4hmhMCf9Vh8MMTgEtwf5ZrO18phs/3kLZ8cPy0QGxQ3m1Wb6hAcHBwYHR61iD6vHdQzjqh5qJewocf5qxhf9ATtKt+3QAhdSQxIyntnMLeHkqghRJTtz0vnVYEEosluY9M7yJ5J1xC807VVLan/+Zbq6P1V5jINnGnkMH9eFO9rXIauc7j2lzrvMX784n0w08JFhfkgOxeGHjtatdjm+fllnzRqh+JGi6cpIcCDA2eapX4WjmbX1zoSFdUfY/4QGh8eHB0c");
        private static int[] order = new int[] { 8,7,3,6,7,13,8,13,11,11,10,13,12,13,14 };
        private static int key = 29;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
