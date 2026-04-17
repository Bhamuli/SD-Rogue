using RogueLib.Dungeon;
using RogueLib.Utilities;

public abstract class Player : IActor, IDrawable {
   public string Name { get; set; }
   public Vector2 Pos;
   public char Glyph => '@';
   public ConsoleColor _color = ConsoleColor.White;

   protected int _level = 1;
   protected int _hp = 18;
   protected int _str = 6;
   protected int _arm = 1;
   protected int _exp = 0;
   protected int _gold = 0;
   protected int _maxHp = 18;
   protected int _maxStr = 6;
   protected int _turn = 0;

   public int Turn => _turn;
   public int Hp => _hp;
   public int MaxHp => _maxHp;
   public int Strength => _str;
   public int Armor => _arm;
   public int Gold => _gold;
   public int Experience => _exp;
   public int LevelNumber => _level;
   public bool IsAlive => _hp > 0;

   public Player() {
      Name = "Rogue";
      Pos = Vector2.Zero;
   }

   public string HUD {
      get {
         string hud =
            $"Lvl:{_level}  Gold:{_gold}  HP:{_hp}/{_maxHp}  Str:{_str}  Arm:{_arm}  Exp:{_exp}/10  Turn:{_turn}";
         return hud.PadRight(78);
      }
   }

   public virtual void AddGold(int amount) {
      if (amount > 0) {
         _gold += amount;
      }
   }

   public virtual void Heal(int amount) {
      if (amount > 0) {
         _hp = Math.Min(_maxHp, _hp + amount);
      }
   }

   public virtual int AttackPower() => Math.Max(1, _str);

   public virtual int TakeDamage(int amount) {
      int actual = Math.Max(1, amount - _arm);
      _hp = Math.Max(0, _hp - actual);
      return actual;
   }

   public virtual void GainExp(int amount) {
      if (amount <= 0) {
         return;
      }

      _exp += amount;
      while (_exp >= 10) {
         _exp -= 10;
         _level++;
         _maxHp += 2;
         _hp = _maxHp;
         _str++;
         _maxStr = _str;
      }
   }

   public virtual void Update() {
      _turn++;
   }

   public virtual void Draw(IRenderWindow disp) {
      disp.Draw(Glyph, Pos, _color);
   }
}
