namespace Celeste.Mod.TSP;

public class TspSettings : EverestModuleSettings {
    public enum HudModes { Hidden, TotalOnly, Full }

    public bool Enabled { get; set; } = true;

    // Distance (en pixels, 8 px = 1 tile) sous laquelle Theo compte comme "proche" (facteur 1).
    [SettingRange(8, 96, true)]
    public int NearRadius { get; set; } = 48;

    // Distance au-delà de laquelle la perte est maximale (perte linéaire entre les deux).
    [SettingRange(64, 400, true)]
    public int FarRadius { get; set; } = 160;

    // Points perdus par seconde lorsque Theo est au-delà du rayon lointain.
    [SettingRange(5, 60, true)]
    public int BleedRate { get; set; } = 25;

    public HudModes HudMode { get; set; } = HudModes.Full;
}
