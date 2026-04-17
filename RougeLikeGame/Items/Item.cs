using RogueLib.Dungeon;
using RogueLib.Utilities;

namespace RlGameNS;

public abstract class Item : IDrawable {
   public string Name { get; protected set; }
   public Vector2 Pos { get; protected set; }
   public char Glyph { get; protected set; }
   public ConsoleColor Color { get; protected set; }
   public bool IsCollected { get; private set; }

   protected Item(string name, Vector2 pos, char glyph, ConsoleColor color) {
      Name = name;
      Pos = pos;
      Glyph = glyph;
      Color = color;
   }

   public void Collect(Player player) {
      if (IsCollected) {
         return;
      }

      OnCollect(player);
      IsCollected = true;
   }

   protected abstract void OnCollect(Player player);
   public abstract string GetPickupMessage();

   public virtual void Draw(IRenderWindow disp) {
      if (!IsCollected) {
         disp.Draw(Glyph, Pos, Color);
      }
   }
}
