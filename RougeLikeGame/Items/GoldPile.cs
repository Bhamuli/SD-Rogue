using RogueLib.Utilities;

namespace RlGameNS;

public class GoldPile : Item {
   private readonly int _amount;

   public GoldPile(Vector2 pos, int amount)
      : base("Gold", pos, '$', ConsoleColor.Yellow) {
      _amount = amount;
   }

   protected override void OnCollect(Player player) {
      player.AddGold(_amount);
   }

   public override string GetPickupMessage() => $"You picked up {_amount} gold.";
}
