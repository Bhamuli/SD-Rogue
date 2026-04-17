using RogueLib.Utilities;

namespace RlGameNS;

public class Potion : Item {
   private readonly int _healAmount;

   public Potion(Vector2 pos, int healAmount)
      : base("Potion", pos, '!', ConsoleColor.Cyan) {
      _healAmount = healAmount;
   }

   protected override void OnCollect(Player player) {
      player.Heal(_healAmount);
   }

   public override string GetPickupMessage() => $"You drink a potion and recover {_healAmount} HP.";
}
