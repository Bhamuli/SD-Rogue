using RogueLib.Engine;

namespace RlGameNS;

class Program {
   static void Main(string[] args) {
      Console.Clear();
      Console.CursorVisible = false;
      Game game = new MyGame();
      game.run();
      Console.ResetColor();
      Console.Clear();
      Console.WriteLine("Thanks for playing our Rogue Like Prototype.");
   }
}
