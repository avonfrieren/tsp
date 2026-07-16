using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.TSP;

public class ScoreHud : Entity {
    private static readonly Color Pink = new(229, 190, 187); // #e5bebb
    private static readonly Color Red = new(214, 72, 72);

    public ScoreHud() {
        Tag = Tags.HUD | Tags.Global | Tags.PauseUpdate | Tags.TransitionUpdate;
        Depth = -100;
    }

    public override void Render() {
        TspSettings settings = TspModule.Settings;
        if (settings == null || !settings.Enabled || settings.HudMode == TspSettings.HudModes.Hidden)
            return;
        TspSession session = TspModule.Session;
        if (session == null || Scene is not Level)
            return;

        // Sous l'emplacement du timer speedrun, coordonnées HUD (1920×1080).
        Vector2 pos = new(32f, 120f);
        ActiveFont.DrawOutline($"TSP : {session.TotalScore:F0}", pos, Vector2.Zero,
            Vector2.One * 0.8f, Pink, 2f, Color.Black);

        ScoreTracker tracker = TspModule.Instance.Tracker;
        if (settings.HudMode == TspSettings.HudModes.Full && tracker.Armed) {
            float ratio = (float)(tracker.CurrentRoomScore / ScoreTracker.BasePoints);
            Color color = Color.Lerp(Red, Pink, ratio);
            ActiveFont.DrawOutline($"{tracker.CurrentRoomScore:F0}/100", pos + new Vector2(0f, 44f),
                Vector2.Zero, Vector2.One * 0.6f, color, 2f, Color.Black);
        }
    }
}
