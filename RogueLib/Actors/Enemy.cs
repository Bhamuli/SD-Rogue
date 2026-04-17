using RogueLib.Dungeon;
using RogueLib.Utilities;

namespace RogueLib.Actors;

public abstract class Enemy : IActor, IDrawable {
   public string Name { get; protected set; }
   public Vector2 Pos { get; set; }
   public char Glyph { get; protected set; }
   public ConsoleColor Color { get; protected set; }

   protected int _hp;
   protected int _strength;
   protected int _armor;
   protected int _goldReward;
   protected int _expReward;

   public int Hp => _hp;
   public int GoldReward => _goldReward;
   public int ExpReward => _expReward;
   public bool IsAlive => _hp > 0;

   protected Enemy(string name, Vector2 pos, char glyph, ConsoleColor color, int hp, int strength, int armor,
      int goldReward, int expReward) {
      Name = name;
      Pos = pos;
      Glyph = glyph;
      Color = color;
      _hp = hp;
      _strength = strength;
      _armor = armor;
      _goldReward = goldReward;
      _expReward = expReward;
   }

   public virtual int AttackPower() => Math.Max(1, _strength - 1);

   public virtual int TakeDamage(int amount) {
      int actual = Math.Max(1, amount - _armor);
      _hp = Math.Max(0, _hp - actual);
      return actual;
   }

   public virtual void Draw(IRenderWindow disp) {
      if (IsAlive) {
         disp.Draw(Glyph, Pos, Color);
      }
   }
}
