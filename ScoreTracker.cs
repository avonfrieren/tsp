using System;
using Monocle;

namespace Celeste.Mod.TSP;

/// <summary>
/// Modèle « cliquet » : le score de la room part de 100 et ne fait que baisser.
/// Tant que Theo est loin de Madeline, le score saigne en continu (proportionnel
/// à l'éloignement et à la durée) ; le rapprocher stoppe l'hémorragie mais ne rend
/// rien. Comme le score ne peut jamais remonter, aucun déplacement ne peut farmer
/// de points — seule solution pour marquer : garder Theo près de soi.
/// </summary>
public class ScoreTracker {
    public const double BasePoints = 100.0;

    private string currentRoom;

    /// <summary>Score courant de la room (0..100), monotone décroissant.</summary>
    public double RoomScore { get; private set; } = BasePoints;

    /// <summary>Le cliquet ne s'active qu'une fois Theo ramassé au moins une fois.</summary>
    public bool Armed { get; private set; }

    public bool TheoInRoom { get; private set; }

    public double CurrentRoomScore => RoomScore;

    public void Reset(Level level) {
        RoomScore = BasePoints;
        Armed = false;
        TheoInRoom = false;
        currentRoom = level?.Session?.Level;
    }

    public void Update(Level level) {
        if (level.Paused || level.Frozen || level.Transitioning || level.InCutscene || level.SkippingCutscene)
            return;

        Player player = level.Tracker.GetEntity<Player>();
        if (player == null || player.Dead)
            return;

        // Room sans Theo (cutscene, avant de le récupérer, abandonné) : rien ne se passe.
        TheoCrystal theo = level.Entities.FindFirst<TheoCrystal>();
        if (theo == null) {
            TheoInRoom = false;
            return;
        }
        TheoInRoom = true;

        // Le cliquet s'arme au premier ramassage : évite de saigner injustement
        // quand Theo spawn loin et qu'il faut d'abord aller le chercher.
        if (theo.Hold.IsHeld)
            Armed = true;
        if (!Armed)
            return;

        double dist = theo.Hold.IsHeld ? 0.0 : (theo.Center - player.Center).Length();

        int near = TspModule.Settings.NearRadius;
        int far = TspModule.Settings.FarRadius;
        if (far <= near)
            far = near + 1;

        double excess = Math.Clamp((dist - near) / (far - near), 0.0, 1.0);
        if (excess > 0.0)
            RoomScore = Math.Max(0.0, RoomScore - TspModule.Settings.BleedRate * excess * Engine.DeltaTime);
    }

    public void Bank(Level level) {
        // Ne banker que les rooms réellement jouées avec Theo (cliquet armé).
        if (Armed && currentRoom != null && TspModule.Session != null) {
            var scores = TspModule.Session.RoomScores;
            if (!scores.TryGetValue(currentRoom, out double best) || RoomScore > best)
                scores[currentRoom] = RoomScore;
        }
        Reset(level);
    }
}
