using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.TSP;

public class TspSession : EverestModuleSession {
    // Meilleur score encaissé par room (clé = nom de la room dans la map).
    public Dictionary<string, double> RoomScores { get; set; } = new();

    public double TotalScore => RoomScores.Values.Sum();
}
