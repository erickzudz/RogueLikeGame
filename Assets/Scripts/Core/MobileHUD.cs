using UnityEngine;

public class MobileHUD : MonoBehaviour
{
    public static MobileHUD I { get; private set; }

    public MobileJoystick joystick;   // arrastra JoyBG
    public MobileButton fireBtn;      // arrastra FireBtn
    public MobileButton abilityBtn;   // arrastra AbilityBtn

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    public bool IsPresent =>
        joystick != null || fireBtn != null || abilityBtn != null;
}
