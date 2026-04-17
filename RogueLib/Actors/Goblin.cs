using RogueLib.Utilities;

namespace RogueLib.Actors;

public class Goblin : Enemy {
   public Goblin(Vector2 pos)
      : base("Goblin", pos, 'g', ConsoleColor.Red, 8, 4, 1, 3, 4) {
   }
}
