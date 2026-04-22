using Logic_Revolver.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicRevolver.Game.Logic
{
    public class GameManager
    {
        public GameState State { get; set; }
        private Stack<ShellType> GunChamber = new Stack<ShellType>();
        private Random _rand = new Random();

        // Biến này giờ sẽ đếm số màn chơi (Level), không phải số lần nạp đạn
        private int _currentMatchRound = 1;

        public ShellType? LastFiredShell { get; private set; } = null;

        public event Action<string> OnLog;
        public event Action OnGameUpdate;
        public event Action OnRoundOver; // Sự kiện hết đạn -> Nạp lại
        public event Action OnMatchEnd;  // Sự kiện hết máu -> Qua màn mới / Kết thúc game

        public GameManager()
        {
            State = new GameState();
        }

        public void StartGame()
        {
            _currentMatchRound = 1; // Bắt đầu từ Round 1

            // Khởi tạo Player
            State.Player1 = new Player("PLAYER 1", 4, false); // Máu khởi điểm
            State.Player2 = new Player("BOT AI", 4, true);
            State.CurrentPlayer = State.Player1;

            OnLog?.Invoke("=== BẮT ĐẦU GAME: ROUND 1 ===");
            LoadGun(); // Nạp đạn lần đầu
        }

        // --- HÀM NẠP ĐẠN (Chạy nhiều lần trong 1 Round) ---
        public void LoadGun()
        {
            GunChamber.Clear();

            // reset viên đạn vừa bắn trước đó
           

            // 1. CẤP VẬT PHẨM (Dựa trên Round hiện tại)
            int itemsToGive = 0;

            // Cấu hình số lượng item theo Round (Màn chơi)
            switch (_currentMatchRound)
            {
                case 1: itemsToGive = 0; break; // Round 1: Không cho gì
                case 2: itemsToGive = 2; break; // Round 2: Cho 2 món
                default: itemsToGive = 4; break; // Round 3+: Cho 4 món (Chiến khô máu)
            }

            // Chỉ cấp đồ nếu không phải Round 1
            if (itemsToGive > 0)
            {
                GiveRandomItems(State.Player1, itemsToGive);
                GiveRandomItems(State.Player2, itemsToGive);
                OnLog?.Invoke($"--- TIẾP TẾ: MỖI BÊN +{itemsToGive} ITEM ---");
            }

            // 2. CẤU HÌNH ĐẠN (Tăng độ khó theo Round)
            int minTotal = 3, maxTotal = 3;
            if (_currentMatchRound == 2) { minTotal = 4; maxTotal = 6; }
            else if (_currentMatchRound >= 3) { minTotal = 6; maxTotal = 8; }

            // Random số đạn
            int total = _rand.Next(minTotal, maxTotal + 1);
            int minLive = (int)Math.Ceiling(total / 2.0); // Đạn thật >= 50%
            int live = _rand.Next(minLive, total); // Random đạn thật
            int blank = total - live;

            State.LiveCount = live;
            State.BlankCount = blank;

            // Xáo trộn đạn
            List<ShellType> shells = new List<ShellType>();
            for (int i = 0; i < live; i++) shells.Add(ShellType.Live);
            for (int i = 0; i < blank; i++) shells.Add(ShellType.Blank);
            shells = shells.OrderBy(x => _rand.Next()).ToList();

            foreach (var s in shells) GunChamber.Push(s);

            OnLog?.Invoke($"--- NẠP ĐẠN (Round {_currentMatchRound}): {live} ĐỎ / {blank} XANH ---");
            OnGameUpdate?.Invoke();
        }

        // --- XỬ LÝ TRẠNG THÁI GAME (Quan trọng) ---
        private void CheckGameState()
        {
            // 1. KIỂM TRA MÁU (Ai chết?)
            if (State.Player1.Hp <= 0 || State.Player2.Hp <= 0)
            {
                HandleRoundEnd(); // Xử lý kết thúc Round
            }
            // 2. KIỂM TRA ĐẠN (Hết đạn nhưng chưa ai chết)
            else if (GunChamber.Count == 0)
            {
                OnRoundOver?.Invoke(); // Gọi sự kiện để Scene báo "Hết đạn" rồi tự animation + LoadGun()
            }
        }

        // --- XỬ LÝ KHI CÓ NGƯỜI CHẾT (Hết Round) ---
        private void HandleRoundEnd()
        {
            bool playerDead = State.Player1.Hp <= 0;
            bool aiDead = State.Player2.Hp <= 0;

            // Nếu chưa ai chết thì thôi
            if (!playerDead && !aiDead) return;

            // ===== ROUND 3: KẾT THÚC THẬT =====
            if (_currentMatchRound >= 3)
            {
                if (playerDead)
                {
                    OnLog?.Invoke($"!!! KẾT THÚC ROUND {_currentMatchRound}: BOT AI THẮNG !!!");
                    OnLog?.Invoke(">>> TRÒ CHƠI KẾT THÚC <<<");
                    OnMatchEnd?.Invoke();
                    return;
                }

                if (aiDead)
                {
                    OnLog?.Invoke($"!!! KẾT THÚC ROUND {_currentMatchRound}: BẠN THẮNG !!!");
                    OnLog?.Invoke(">>> TRÒ CHƠI KẾT THÚC <<<");
                    OnMatchEnd?.Invoke();
                    return;
                }
            }

            // ===== ROUND 1-2: AI CHẾT HOẶC PLAYER CHẾT ĐỀU QUA ROUND TIẾP =====
            if (playerDead)
            {
                OnLog?.Invoke($"!!! KẾT THÚC ROUND {_currentMatchRound}: BẠN THUA HIỆP NÀY !!!");
            }
            else if (aiDead)
            {
                OnLog?.Invoke($"!!! KẾT THÚC ROUND {_currentMatchRound}: BẠN THẮNG HIỆP NÀY !!!");
            }

            _currentMatchRound++;

            // --- RESET TRẠNG THÁI CHO ROUND MỚI ---
            // 1. Hồi đầy máu
            State.Player1.Hp = State.Player1.MaxHp;
            State.Player2.Hp = State.Player2.MaxHp;

            // 2. Xóa sạch túi đồ cũ (Luật game gốc: Qua màn là mất đồ cũ)
            State.Player1.Inventory.Clear();
            State.Player2.Inventory.Clear();

            // 3. Reset các hiệu ứng
            State.DamageMultiplier = 1;
            State.Player1.IsHandcuffed = false;
            State.Player2.IsHandcuffed = false;
            State.KnownShell = null;

            // 4. Reset lượt cho round mới
            State.CurrentPlayer = State.Player1;

            OnLog?.Invoke($"=== BẮT ĐẦU ROUND {_currentMatchRound} ===");

            LoadGun();
        }

        public void Shoot(Target target)
        {
            if (GunChamber.Count == 0) return;

            ShellType shell = GunChamber.Pop();
            LastFiredShell = shell;

            if (shell == ShellType.Live) State.LiveCount--;
            else State.BlankCount--;

            // Chặn âm
            State.LiveCount = Math.Max(0, State.LiveCount);
            State.BlankCount = Math.Max(0, State.BlankCount);

            State.KnownShell = null;

            string targetName = (target == Target.Self) ? "mình" : "địch";
            OnLog?.Invoke($"{State.CurrentPlayer.Name} bắn {targetName}...");

            if (shell == ShellType.Live)
            {
                OnLog?.Invoke(">> ĐOÀNG! Đạn thật!");
                int dmg = 1 * State.DamageMultiplier;
                State.DamageMultiplier = 1;

                Player victim = (target == Target.Self) ? State.CurrentPlayer : State.GetOpponent(State.CurrentPlayer);
                victim.Hp -= dmg;
                if (victim.Hp < 0) victim.Hp = 0;

                SwitchTurn();
            }
            else
            {
                OnLog?.Invoke(">> TẠCH. Đạn rỗng.");
                State.DamageMultiplier = 1;

                if (target != Target.Self) SwitchTurn();
                else OnLog?.Invoke(">> Được thêm lượt!");
            }

            CheckGameState();
            OnGameUpdate?.Invoke();
        }

        public void UseItem(Player p, ItemType item)
        {
            if (!p.Inventory.Contains(item)) return;
            p.Inventory.Remove(item);
            OnLog?.Invoke($"{p.Name} dùng {item}");

            switch (item)
            {
                case ItemType.Beer:
                    if (GunChamber.Count > 0)
                    {
                        ShellType s = GunChamber.Pop();
                        LastFiredShell = null;
                        OnLog?.Invoke($">> Đạn văng ra: {(s == ShellType.Live ? "ĐỎ" : "XANH")}");

                        if (s == ShellType.Live) State.LiveCount--;
                        else State.BlankCount--;

                        // Chặn âm
                        State.LiveCount = Math.Max(0, State.LiveCount);
                        State.BlankCount = Math.Max(0, State.BlankCount);

                        State.KnownShell = null;
                    }
                    break;

                case ItemType.Knife:
                    State.DamageMultiplier = 2;
                    OnLog?.Invoke(">> Sát thương x2");
                    break;

                case ItemType.Glass:
                    if (GunChamber.Count > 0)
                    {
                        State.KnownShell = GunChamber.Peek();
                        OnLog?.Invoke($">> Soi thấy: {(State.KnownShell == ShellType.Live ? "ĐỎ" : "XANH")}");
                    }
                    break;

                case ItemType.Handcuffs:
                    State.GetOpponent(p).IsHandcuffed = true;
                    OnLog?.Invoke(">> Địch bị khóa lượt sau");
                    break;

                case ItemType.Cigarette:
                    if (p.Hp < p.MaxHp)
                    {
                        p.Hp++; // Tăng 1 máu
                        OnLog?.Invoke($">> {p.Name} hút thuốc: Hồi 1 máu (HP: {p.Hp})");
                    }
                    else
                    {
                        OnLog?.Invoke($">> {p.Name} hút thuốc cho ngầu (Máu đã đầy)");
                    }
                    break;
            }

            CheckGameState();
            OnGameUpdate?.Invoke();
        }

        private void SwitchTurn()
        {
            Player opponent = State.GetOpponent(State.CurrentPlayer);
            if (opponent.IsHandcuffed)
            {
                OnLog?.Invoke($">> {opponent.Name} bị còng, mất lượt!");
                opponent.IsHandcuffed = false;
            }
            else
            {
                State.CurrentPlayer = opponent;
            }
        }

        private void GiveRandomItems(Player p, int count)
        {
            Array values = Enum.GetValues(typeof(ItemType));
            bool isFull = false;

            for (int i = 0; i < count; i++)
            {
                if (p.Inventory.Count >= 8)
                {
                    isFull = true;
                    break;
                }
                ItemType randomItem = (ItemType)values.GetValue(_rand.Next(values.Length));
                p.Inventory.Add(randomItem);
            }
            if (isFull) OnLog?.Invoke($"! TÚI ĐỒ {p.Name} ĐẦY !");
        }
    }
}