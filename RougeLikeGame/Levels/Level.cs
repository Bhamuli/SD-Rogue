using RogueLib.Actors;
using RogueLib.Dungeon;
using RogueLib.Engine;
using RogueLib.Utilities;
using TileSet = System.Collections.Generic.HashSet<RogueLib.Utilities.Vector2>;

namespace RlGameNS;

public class Level : Scene {
   protected string? _map;
   protected int _senseRadius = 6;

   protected TileSet _walkables;
   protected TileSet _floor;
   protected TileSet _tunnel;
   protected TileSet _door;
   protected TileSet _decor;
   protected TileSet _discovered;
   protected TileSet _inFov;

   private readonly List<Enemy> _enemies;
   private readonly List<Item> _items;
   private readonly Random _rng;

   private Vector2 _exitPos;
   private bool _gameEnded;
   private bool _playerWon;
   private string _message;
   private readonly int _goldNeededToExit;

   public Level(Player p, string map, Game game) {
      if (game == null || p == null || map == null) {
         throw new ArgumentNullException("game, player, or map cannot be null");
      }

      _player = p;
      _map = map;
      _game = game;
      _rng = new Random();
      _message = "Collect gold, avoid goblins, and reach the exit >.";
      _goldNeededToExit = 10;
      _gameEnded = false;
      _playerWon = false;

      _walkables = new TileSet();
      _floor = new TileSet();
      _tunnel = new TileSet();
      _door = new TileSet();
      _decor = new TileSet();
      _discovered = new TileSet();
      _inFov = new TileSet();
      _enemies = new List<Enemy>();
      _items = new List<Item>();

      InitMapTileSets(map);
      SeedLevelContent();
      UpdateDiscovered();
      RegisterCommandsWithScene();
   }

   public override void Update() {
      if (_gameEnded) {
         return;
      }

      MoveEnemies();
      RemoveDeadEnemies();
      _player!.Update();

      if (!_player.IsAlive) {
         _gameEnded = true;
         _playerWon = false;
         _message = "You were defeated. Press Q to quit.";
      }
   }

   public override void Draw(IRenderWindow disp) {
      var tilesToDraw = new TileSet(_decor);
      tilesToDraw.IntersectWith(_discovered);
      tilesToDraw.UnionWith(_inFov);

      disp.fDraw(tilesToDraw, _map!, ConsoleColor.Gray);
      disp.Draw('>', _exitPos, ConsoleColor.Green);

      DrawItems(disp);
      DrawEnemies(disp);
      DrawPlayer(disp);

      disp.Draw(BuildMessageLine(), new Vector2(0, 0), ConsoleColor.White);
      disp.Draw(_player!.HUD, new Vector2(0, 24), ConsoleColor.Green);
   }

   public override void DoCommand(Command command) {
      if (command.Name == "quit") {
         _levelActive = false;
         return;
      }

      if (_gameEnded) {
         return;
      }

      if (command.Name == "up") {
         MovePlayer(Vector2.N);
      } else if (command.Name == "down") {
         MovePlayer(Vector2.S);
      } else if (command.Name == "left") {
         MovePlayer(Vector2.W);
      } else if (command.Name == "right") {
         MovePlayer(Vector2.E);
      }
   }

   private void DrawPlayer(IRenderWindow disp) {
      _player!._color = _player.Turn % 2 == 0 ? ConsoleColor.White : ConsoleColor.Cyan;
      _player.Draw(disp);
   }

   private void DrawItems(IRenderWindow disp) {
      foreach (var item in _items) {
         if (!item.IsCollected && _inFov.Contains(item.Pos)) {
            item.Draw(disp);
         }
      }
   }

   private void DrawEnemies(IRenderWindow disp) {
      foreach (var enemy in _enemies) {
         if (enemy.IsAlive && _inFov.Contains(enemy.Pos)) {
            enemy.Draw(disp);
         }
      }
   }

   private void InitMapTileSets(string map) {
      foreach (var (c, p) in Vector2.Parse(map)) {
         if (c == '.') {
            _floor.Add(p);
         } else if (c == '+') {
            _door.Add(p);
         } else if (c == '#') {
            _tunnel.Add(p);
         } else if (c != ' ') {
            _decor.Add(p);
         }
      }

      _walkables = _floor.Union(_tunnel).Union(_door).ToHashSet();
      _decor.UnionWith(_door);
      _decor.UnionWith(_tunnel);
      _decor.UnionWith(_floor);
   }

   private void SeedLevelContent() {
      var orderedFloors = _floor.OrderBy(t => t.Y).ThenBy(t => t.X).ToList();
      if (orderedFloors.Count < 12) {
         throw new InvalidOperationException("Map does not contain enough walkable floor tiles.");
      }

      _player!.Pos = orderedFloors[2];
      _exitPos = orderedFloors[^3];

      AddItem(new GoldPile(orderedFloors[8], 3));
      AddItem(new GoldPile(orderedFloors[20], 4));
      AddItem(new GoldPile(orderedFloors[36], 5));
      AddItem(new Potion(orderedFloors[16], 5));
      AddItem(new Potion(orderedFloors[44], 4));

      AddEnemy(new Goblin(orderedFloors[28]));
      AddEnemy(new Goblin(orderedFloors[40]));
      AddEnemy(new Goblin(orderedFloors[52]));
   }

   private void AddItem(Item item) {
      if (item.Pos != _player!.Pos && item.Pos != _exitPos) {
         _items.Add(item);
      }
   }

   private void AddEnemy(Enemy enemy) {
      if (enemy.Pos != _player!.Pos && enemy.Pos != _exitPos) {
         _enemies.Add(enemy);
      }
   }

   private void RegisterCommandsWithScene() {
      RegisterCommand(ConsoleKey.UpArrow, "up");
      RegisterCommand(ConsoleKey.W, "up");
      RegisterCommand(ConsoleKey.K, "up");

      RegisterCommand(ConsoleKey.DownArrow, "down");
      RegisterCommand(ConsoleKey.S, "down");
      RegisterCommand(ConsoleKey.J, "down");

      RegisterCommand(ConsoleKey.LeftArrow, "left");
      RegisterCommand(ConsoleKey.A, "left");
      RegisterCommand(ConsoleKey.H, "left");

      RegisterCommand(ConsoleKey.RightArrow, "right");
      RegisterCommand(ConsoleKey.D, "right");
      RegisterCommand(ConsoleKey.L, "right");

      RegisterCommand(ConsoleKey.Q, "quit");
   }

   private void UpdateDiscovered() {
      _inFov = FovCalc(_player!.Pos, _senseRadius);
      _discovered.UnionWith(_inFov);
   }

   private TileSet FovCalc(Vector2 pos, int radius)
      => Vector2.getAllTiles(Game.width, Game.height)
         .Where(t => (pos - t).KingLength <= radius)
         .ToHashSet();

   private void MovePlayer(Vector2 delta) {
      var newPos = _player!.Pos + delta;

      if (!_walkables.Contains(newPos) && newPos != _exitPos) {
         _message = "You bump into a wall.";
         return;
      }

      var enemy = GetEnemyAt(newPos);
      if (enemy is not null) {
         PlayerAttack(enemy);
         return;
      }

      _player.Pos = newPos;
      UpdateDiscovered();
      CheckForItemPickup();
      CheckForExit();
   }

   private void PlayerAttack(Enemy enemy) {
      int dealt = enemy.TakeDamage(_player!.AttackPower());
      _message = $"You hit the {enemy.Name} for {dealt} damage.";

      if (!enemy.IsAlive) {
         _player.AddGold(enemy.GoldReward);
         _player.GainExp(enemy.ExpReward);
         _message = $"You defeated a {enemy.Name}. +{enemy.GoldReward} gold, +{enemy.ExpReward} exp.";
      }
   }

   private void MoveEnemies() {
      foreach (var enemy in _enemies.Where(e => e.IsAlive)) {
         int distance = (enemy.Pos - _player!.Pos).KingLength;

         if (distance <= 1) {
            EnemyAttack(enemy);
            if (!_player.IsAlive) {
               return;
            }

            continue;
         }

         if (distance <= 7) {
            var step = GetStepToward(enemy.Pos, _player.Pos);
            var nextPos = enemy.Pos + step;
            if (CanEnemyMoveTo(nextPos)) {
               enemy.Pos = nextPos;
            }
         } else if (_rng.Next(0, 5) == 0) {
            var randomSteps = new[] { Vector2.N, Vector2.S, Vector2.E, Vector2.W };
            var step = randomSteps[_rng.Next(randomSteps.Length)];
            var nextPos = enemy.Pos + step;
            if (CanEnemyMoveTo(nextPos)) {
               enemy.Pos = nextPos;
            }
         }
      }
   }

   private void EnemyAttack(Enemy enemy) {
      int dealt = _player!.TakeDamage(enemy.AttackPower());
      _message = $"The {enemy.Name} hits you for {dealt} damage.";
   }

   private Vector2 GetStepToward(Vector2 from, Vector2 to) {
      int dx = Math.Sign(to.X - from.X);
      int dy = Math.Sign(to.Y - from.Y);

      if (dx != 0) {
         return new Vector2(dx, 0);
      }

      return new Vector2(0, dy);
   }

   private void CheckForItemPickup() {
      var item = _items.FirstOrDefault(i => !i.IsCollected && i.Pos == _player!.Pos);
      if (item is null) {
         return;
      }

      item.Collect(_player!);
      _message = item.GetPickupMessage();
   }

   private void CheckForExit() {
      if (_player!.Pos != _exitPos) {
         return;
      }

      if (_player.Gold >= _goldNeededToExit) {
         _gameEnded = true;
         _playerWon = true;
         _message = "You escaped the dungeon with enough gold. Press Q to quit.";
      } else {
         _message = $"The exit is sealed. You need {_goldNeededToExit - _player.Gold} more gold.";
      }
   }

   private Enemy? GetEnemyAt(Vector2 pos)
      => _enemies.FirstOrDefault(e => e.IsAlive && e.Pos == pos);

   private bool CanEnemyMoveTo(Vector2 pos) {
      if (!_walkables.Contains(pos)) {
         return false;
      }

      if (pos == _player!.Pos || pos == _exitPos) {
         return false;
      }

      return _enemies.All(e => !e.IsAlive || e.Pos != pos);
   }

   private void RemoveDeadEnemies() {
      _enemies.RemoveAll(e => !e.IsAlive);
   }

   private string BuildMessageLine() {
      string status = _playerWon ? "[Victory] " : _gameEnded ? "[Game Over] " : string.Empty;
      string line = status + _message;
      if (line.Length > Game.width) {
         line = line[..Game.width];
      }

      return line.PadRight(Game.width);
   }
}
