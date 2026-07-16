using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.TSP;

public class TspModule : EverestModule {
    public static TspModule Instance { get; private set; }

    public override Type SettingsType => typeof(TspSettings);
    public static TspSettings Settings => (TspSettings)Instance._Settings;

    public override Type SessionType => typeof(TspSession);
    public static TspSession Session => (TspSession)Instance._Session;

    public ScoreTracker Tracker { get; } = new();

    public TspModule() {
        Instance = this;
    }

    public override void Load() {
        Everest.Events.Level.OnLoadLevel += OnLoadLevel;
        Everest.Events.Level.OnTransitionTo += OnTransitionTo;
        Everest.Events.Level.OnComplete += OnComplete;
        Everest.Events.Player.OnDie += OnDie;
        On.Celeste.Level.Update += OnLevelUpdate;
        Logger.Log(LogLevel.Info, "TSP", "Theo Score Points loaded.");
    }

    public override void Unload() {
        Everest.Events.Level.OnLoadLevel -= OnLoadLevel;
        Everest.Events.Level.OnTransitionTo -= OnTransitionTo;
        Everest.Events.Level.OnComplete -= OnComplete;
        Everest.Events.Player.OnDie -= OnDie;
        On.Celeste.Level.Update -= OnLevelUpdate;
    }

    private void OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader) {
        Tracker.Reset(level);
        if (level.Entities.FindFirst<ScoreHud>() == null)
            level.Add(new ScoreHud());
    }

    // Le score de la room quittée est encaissé au début de la transition ;
    // les compteurs repartent à zéro pour la room suivante.
    private void OnTransitionTo(Level level, LevelData next, Vector2 direction) {
        Tracker.Bank(level);
    }

    // Dernière room du chapitre : pas de transition, on encaisse à la complétion.
    private void OnComplete(Level level) {
        Tracker.Bank(level);
    }

    private void OnDie(Player player) {
        Tracker.Reset(player.Scene as Level);
    }

    private void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
        orig(self);
        if (Settings.Enabled)
            Tracker.Update(self);
    }
}
