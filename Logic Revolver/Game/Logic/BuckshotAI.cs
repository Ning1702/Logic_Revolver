using Logic_Revolver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic_Revolver.Game.Logic
{
    public class BuckshotAI
    {
        // ============================================================
        // AI này được nâng cấp theo hướng "tìm kiếm trạng thái" như trong
        // các chương về không gian trạng thái, tìm kiếm kinh nghiệm,
        // tìm kiếm tối ưu và tìm kiếm có đối thủ.
        //
        // Ý tưởng áp dụng vào game:
        // 1. Mỗi tình huống hiện tại của ván Buckshot được xem là một trạng thái.
        // 2. Mỗi hành động hợp lệ (bắn, dùng item...) là một toán tử sinh trạng thái mới.
        // 3. AI dùng hàm đánh giá để chấm điểm trạng thái.
        // 4. AI duyệt cây trò chơi bằng Minimax + Alpha-Beta để chọn nước đi tốt nhất.
        // 5. Dùng "sâu lặp" (iterative deepening) để tăng dần độ sâu tìm kiếm.
        //
        // Lưu ý:
        // - Buckshot có yếu tố ngẫu nhiên (đạn thật / đạn lép khi chưa biết viên hiện tại),
        //   vì vậy khi mô phỏng hành động, AI dùng giá trị kỳ vọng trên các nhánh xác suất.
        // - Đây là cách "lai" giữa tìm kiếm đối kháng và đánh giá xác suất để phù hợp gameplay.
        // ============================================================

        private const int MAX_SEARCH_DEPTH = 4;
        private const double WIN_SCORE = 100000.0;
        private const double LOSE_SCORE = -100000.0;

        public string GetBestMove(GameState state)
        {
            var root = AIState.FromGameState(state);

            if (root.TotalShells == 0)
                return "WAIT";

            // ============================================================
            // TÌM KIẾM SÂU LẶP:
            // - Bắt đầu từ độ sâu nhỏ
            // - Tăng dần tới MAX_SEARCH_DEPTH
            // - Mỗi lần đều tìm nước đi tốt nhất tại độ sâu hiện tại
            //
            // Ý nghĩa trong game:
            // - Depth nhỏ: phản ứng nhanh
            // - Depth lớn hơn: thấy được combo sâu hơn như
            //   Còng tay -> Dao -> Bắn -> giữ lượt
            // ============================================================
            string bestMove = "WAIT";

            for (int depth = 1; depth <= MAX_SEARCH_DEPTH; depth++)
            {
                double alpha = double.NegativeInfinity;
                double beta = double.PositiveInfinity;
                double bestValue = double.NegativeInfinity;

                var actions = OrderActions(root, GenerateActions(root));

                foreach (var action in actions)
                {
                    double value = EvaluateAction(root, action, depth, alpha, beta);

                    if (value > bestValue)
                    {
                        bestValue = value;
                        bestMove = action;
                    }

                    alpha = Math.Max(alpha, bestValue);
                }
            }

            return bestMove;
        }

        // ============================================================
        // GIAI ĐOẠN SINH HÀNH ĐỘNG:
        // Giữ tinh thần comment cũ:
        // - Giai đoạn 1: Thu thập thông tin / hồi phục
        // - Giai đoạn 2: Nếu đã biết đạn thì combo tối ưu
        // - Giai đoạn 3: Nếu chưa biết thì dùng xác suất
        // - Giai đoạn 4: Quyết định bắn
        //
        // Khác biệt:
        // - Trước đây comment cũ được dùng như "luật cứng"
        // - Bây giờ vẫn giữ logic ưu tiên đó, nhưng biến thành
        //   "thứ tự sinh + sắp xếp hành động" để phục vụ minimax.
        // ============================================================
        private List<string> GenerateActions(AIState s)
        {
            var actions = new List<string>();

            if (s.TotalShells <= 0)
            {
                actions.Add("WAIT");
                return actions;
            }

            bool aiTurn = s.AiTurn;
            var inventory = aiTurn ? s.AiInventory : s.PlayerInventory;
            int hp = aiTurn ? s.AiHp : s.PlayerHp;
            int maxHp = aiTurn ? s.AiMaxHp : s.PlayerMaxHp;
            bool opponentCuffed = aiTurn ? s.PlayerCuffed : s.AiCuffed;

            // --- GIAI ĐOẠN 1: THU THẬP THÔNG TIN & HỒI PHỤC ---

            // 1. Soi đạn (Glass): Ưu tiên cao nếu chưa biết gì
            if (inventory.Contains(ItemType.Glass) && s.KnownShell == null)
                actions.Add("USE_GLASS");

            // 2. Hồi máu (Thuốc lá): Luôn nên xét nếu mất máu
            if (inventory.Contains(ItemType.Cigarette) && hp < maxHp)
                actions.Add("USE_CIGARETTE");

            // Bia luôn là một nước hợp lệ nếu còn đạn
            if (inventory.Contains(ItemType.Beer))
                actions.Add("USE_BEER");

            // Còng tay: xét nếu đối thủ chưa bị khóa
            if (inventory.Contains(ItemType.Handcuffs) && !opponentCuffed)
                actions.Add("USE_HANDCUFFS");

            // Dao: chỉ có ích khi chưa buff sát thương
            if (inventory.Contains(ItemType.Knife) && s.DamageMultiplier == 1)
                actions.Add("USE_KNIFE");

            // --- GIAI ĐOẠN 4: QUYẾT ĐỊNH BẮN ---
            actions.Add("SHOOT_OPPONENT");
            actions.Add("SHOOT_SELF");

            if (actions.Count == 0)
                actions.Add("WAIT");

            return actions;
        }

        // ============================================================
        // SẮP XẾP HÀNH ĐỘNG THEO HEURISTIC (Best-First / A* style)
        //
        // Tinh thần tài liệu:
        // - Tìm kiếm tốt nhất đầu tiên: chọn trạng thái / nhánh hứa hẹn nhất
        // - A*: dùng hàm f = g + h để ưu tiên phát triển nhánh tốt hơn
        //
        // Áp dụng trong game:
        // - Không dùng A* thuần để trả lời trực tiếp nước đi
        // - Mà dùng "action ordering" để alpha-beta cắt được nhiều nhánh hơn
        // - Nhánh hứa hẹn hơn sẽ được xét trước
        // ============================================================
        private List<string> OrderActions(AIState s, List<string> actions)
        {
            return actions
                .OrderByDescending(a => ActionPriority(s, a))
                .ToList();
        }

        private double ActionPriority(AIState s, string action)
        {
            double pLive = s.LiveProbability;

            if (s.KnownShell != null)
            {
                if (s.KnownShell == ShellType.Live)
                {
                    if (action == "USE_HANDCUFFS") return 120;
                    if (action == "USE_KNIFE" && s.DamageMultiplier == 1) return 115;
                    if (action == "SHOOT_OPPONENT") return 110;
                    if (action == "USE_CIGARETTE") return 80;
                    if (action == "USE_GLASS") return 10;
                    if (action == "SHOOT_SELF") return -100;
                }
                else
                {
                    if (action == "SHOOT_SELF") return 100;
                    if (action == "USE_BEER") return 70;
                    if (action == "USE_CIGARETTE") return 60;
                    if (action == "SHOOT_OPPONENT") return -30;
                }
            }

            // --- GIAI ĐOẠN 3: XỬ LÝ KHI MÙ TỊT (DỰA VÀO XÁC SUẤT) ---

            if (action == "USE_GLASS" && s.KnownShell == null) return 95;
            if (action == "USE_CIGARETTE" && ((s.AiTurn ? s.AiHp < s.AiMaxHp : s.PlayerHp < s.PlayerMaxHp))) return 90;
            if (action == "USE_HANDCUFFS" && pLive >= 0.5) return 88;
            if (action == "USE_KNIFE" && pLive >= 0.6 && s.DamageMultiplier == 1) return 84;
            if (action == "USE_BEER" && pLive < 0.4) return 82;

            if (action == "SHOOT_OPPONENT")
                return pLive >= 0.5 ? 75 + (pLive * 10.0) : 40 + (pLive * 10.0);

            if (action == "SHOOT_SELF")
                return pLive < 0.5 ? 74 + ((1.0 - pLive) * 10.0) : 20;

            return 0;
        }

        // ============================================================
        // ĐÁNH GIÁ 1 HÀNH ĐỘNG:
        // - Hành động có thể sinh 1 trạng thái chắc chắn
        // - Hoặc nhiều trạng thái nếu có yếu tố xác suất
        // - Giá trị cuối cùng là kỳ vọng xác suất
        // ============================================================
        private double EvaluateAction(AIState s, string action, int depth, double alpha, double beta)
        {
            var outcomes = ApplyAction(s, action);
            double total = 0.0;

            foreach (var outcome in outcomes)
            {
                double value = AlphaBeta(outcome.State, depth - 1, alpha, beta);
                total += outcome.Probability * value;
            }

            return total;
        }

        // ============================================================
        // MINIMAX + ALPHA-BETA
        //
        // Áp dụng trong game:
        // - Nút MAX: lượt AI
        // - Nút MIN: lượt Player
        // - Duyệt tới độ sâu giới hạn
        // - Lá cây dùng hàm đánh giá heuristic
        // - Alpha/Beta để bỏ các nhánh chắc chắn không tốt hơn
        // ============================================================
        private double AlphaBeta(AIState s, int depth, double alpha, double beta)
        {
            if (IsTerminal(s, out double terminalScore))
                return terminalScore;

            if (depth <= 0)
                return EvaluateState(s);

            var actions = OrderActions(s, GenerateActions(s));

            if (actions.Count == 0)
                return EvaluateState(s);

            if (s.AiTurn) // MAX
            {
                double value = double.NegativeInfinity;

                foreach (var action in actions)
                {
                    double expected = 0.0;
                    var outcomes = ApplyAction(s, action);

                    foreach (var outcome in outcomes)
                        expected += outcome.Probability * AlphaBeta(outcome.State, depth - 1, alpha, beta);

                    value = Math.Max(value, expected);
                    alpha = Math.Max(alpha, value);

                    if (alpha >= beta)
                        break; // beta-cut
                }

                return value;
            }
            else // MIN
            {
                double value = double.PositiveInfinity;

                foreach (var action in actions)
                {
                    double expected = 0.0;
                    var outcomes = ApplyAction(s, action);

                    foreach (var outcome in outcomes)
                        expected += outcome.Probability * AlphaBeta(outcome.State, depth - 1, alpha, beta);

                    value = Math.Min(value, expected);
                    beta = Math.Min(beta, value);

                    if (alpha >= beta)
                        break; // alpha-cut
                }

                return value;
            }
        }

        // ============================================================
        // HÀM ĐÁNH GIÁ TRẠNG THÁI
        //
        // Tinh thần tài liệu:
        // - Trong tìm kiếm kinh nghiệm / A*, ta cần hàm h hoặc f=g+h
        // - Trong minimax độ sâu hữu hạn, ta cần hàm lượng giá lá tạm thời
        //
        // Áp dụng vào Buckshot:
        // - Ưu thế máu
        // - Ưu thế item
        // - Trạng thái biết / chưa biết viên đạn
        // - Xác suất viên thật
        // - Còng tay
        // - Hệ số sát thương
        // - Lượt chơi hiện tại
        // ============================================================
        private double EvaluateState(AIState s)
        {
            if (IsTerminal(s, out double terminalScore))
                return terminalScore;

            double score = 0.0;

            // Máu là yếu tố quan trọng nhất
            score += (s.AiHp - s.PlayerHp) * 120.0;

            // Lợi thế item
            score += EvaluateInventory(s.AiInventory) * 12.0;
            score -= EvaluateInventory(s.PlayerInventory) * 12.0;

            // Lợi thế còng tay
            if (s.PlayerCuffed) score += 30.0;
            if (s.AiCuffed) score -= 30.0;

            // Lợi thế hệ số sát thương
            if (s.DamageMultiplier > 1)
                score += s.AiTurn ? 20.0 : -20.0;

            // Đạn và tri thức về viên hiện tại
            if (s.TotalShells > 0)
            {
                double pLive = s.LiveProbability;

                if (s.KnownShell != null)
                {
                    if (s.KnownShell == ShellType.Live)
                    {
                        score += s.AiTurn ? 40.0 : -40.0;

                        if (s.DamageMultiplier > 1)
                            score += s.AiTurn ? 18.0 : -18.0;
                    }
                    else
                    {
                        score += s.AiTurn ? 18.0 : -18.0;
                    }
                }
                else
                {
                    // Khi chưa biết viên đạn, phía nào đang cầm lượt mà pLive cao hơn
                    // thì có xu hướng ép được đối thủ tốt hơn.
                    score += s.AiTurn ? (pLive - 0.5) * 50.0 : -(pLive - 0.5) * 50.0;
                }
            }

            // Thưởng nhẹ nếu AI có thể kết liễu sớm
            if (s.PlayerHp <= s.DamageMultiplier && s.TotalShells > 0)
            {
                if (s.KnownShell == ShellType.Live)
                    score += s.AiTurn ? 80.0 : -80.0;
                else
                    score += s.AiTurn ? s.LiveProbability * 50.0 : -(s.LiveProbability * 50.0);
            }

            return score;
        }

        private double EvaluateInventory(List<ItemType> inventory)
        {
            double score = 0.0;

            foreach (var item in inventory)
            {
                switch (item)
                {
                    case ItemType.Glass:
                        score += 2.5;
                        break;
                    case ItemType.Handcuffs:
                        score += 3.0;
                        break;
                    case ItemType.Knife:
                        score += 2.2;
                        break;
                    case ItemType.Cigarette:
                        score += 2.0;
                        break;
                    case ItemType.Beer:
                        score += 1.6;
                        break;
                }
            }

            return score;
        }

        private bool IsTerminal(AIState s, out double score)
        {
            if (s.PlayerHp <= 0)
            {
                score = WIN_SCORE;
                return true;
            }

            if (s.AiHp <= 0)
            {
                score = LOSE_SCORE;
                return true;
            }

            score = 0;
            return false;
        }

        // ============================================================
        // MÔ PHỎNG HÀNH ĐỘNG
        //
        // Với hành động xác định:
        // - trả về 1 trạng thái mới
        //
        // Với hành động còn ngẫu nhiên:
        // - trả về nhiều trạng thái mới với xác suất tương ứng
        //
        // Các hành động được mô phỏng:
        // - USE_GLASS
        // - USE_CIGARETTE
        // - USE_BEER
        // - USE_HANDCUFFS
        // - USE_KNIFE
        // - SHOOT_OPPONENT
        // - SHOOT_SELF
        // ============================================================
        private List<Outcome> ApplyAction(AIState s, string action)
        {
            switch (action)
            {
                case "USE_GLASS":
                    return ApplyGlass(s);

                case "USE_CIGARETTE":
                    return ApplyCigarette(s);

                case "USE_BEER":
                    return ApplyBeer(s);

                case "USE_HANDCUFFS":
                    return ApplyHandcuffs(s);

                case "USE_KNIFE":
                    return ApplyKnife(s);

                case "SHOOT_OPPONENT":
                    return ApplyShot(s, shootSelf: false);

                case "SHOOT_SELF":
                    return ApplyShot(s, shootSelf: true);

                default:
                    return new List<Outcome>
                    {
                        new Outcome(s.Clone(), 1.0)
                    };
            }
        }

        private List<Outcome> ApplyGlass(AIState s)
        {
            var result = new List<Outcome>();
            var inventory = s.AiTurn ? s.AiInventory : s.PlayerInventory;

            if (!inventory.Contains(ItemType.Glass) || s.TotalShells <= 0)
                return new List<Outcome> { new Outcome(s.Clone(), 1.0) };

            if (s.KnownShell != null)
                return new List<Outcome> { new Outcome(s.CloneWithoutItem(ItemType.Glass), 1.0) };

            double pLive = s.LiveProbability;
            double pBlank = 1.0 - pLive;

            if (s.LiveCount > 0)
            {
                var a = s.CloneWithoutItem(ItemType.Glass);
                a.KnownShell = ShellType.Live;
                result.Add(new Outcome(a, pLive));
            }

            if (s.BlankCount > 0)
            {
                var b = s.CloneWithoutItem(ItemType.Glass);
                b.KnownShell = ShellType.Blank;
                result.Add(new Outcome(b, pBlank));
            }

            return NormalizeOutcomes(result);
        }

        private List<Outcome> ApplyCigarette(AIState s)
        {
            var inventory = s.AiTurn ? s.AiInventory : s.PlayerInventory;

            if (!inventory.Contains(ItemType.Cigarette))
                return new List<Outcome> { new Outcome(s.Clone(), 1.0) };

            var next = s.CloneWithoutItem(ItemType.Cigarette);

            if (s.AiTurn)
                next.AiHp = Math.Min(next.AiHp + 1, next.AiMaxHp);
            else
                next.PlayerHp = Math.Min(next.PlayerHp + 1, next.PlayerMaxHp);

            return new List<Outcome> { new Outcome(next, 1.0) };
        }

        private List<Outcome> ApplyHandcuffs(AIState s)
        {
            var inventory = s.AiTurn ? s.AiInventory : s.PlayerInventory;

            if (!inventory.Contains(ItemType.Handcuffs))
                return new List<Outcome> { new Outcome(s.Clone(), 1.0) };

            var next = s.CloneWithoutItem(ItemType.Handcuffs);

            if (s.AiTurn)
                next.PlayerCuffed = true;
            else
                next.AiCuffed = true;

            return new List<Outcome> { new Outcome(next, 1.0) };
        }

        private List<Outcome> ApplyKnife(AIState s)
        {
            var inventory = s.AiTurn ? s.AiInventory : s.PlayerInventory;

            if (!inventory.Contains(ItemType.Knife))
                return new List<Outcome> { new Outcome(s.Clone(), 1.0) };

            var next = s.CloneWithoutItem(ItemType.Knife);
            next.DamageMultiplier = 2;

            return new List<Outcome> { new Outcome(next, 1.0) };
        }

        private List<Outcome> ApplyBeer(AIState s)
        {
            var inventory = s.AiTurn ? s.AiInventory : s.PlayerInventory;

            if (!inventory.Contains(ItemType.Beer) || s.TotalShells <= 0)
                return new List<Outcome> { new Outcome(s.Clone(), 1.0) };

            var result = new List<Outcome>();

            if (s.KnownShell != null)
            {
                var next = s.CloneWithoutItem(ItemType.Beer);

                if (s.KnownShell == ShellType.Live)
                    next.LiveCount = Math.Max(0, next.LiveCount - 1);
                else
                    next.BlankCount = Math.Max(0, next.BlankCount - 1);

                next.KnownShell = null;
                result.Add(new Outcome(next, 1.0));
                return result;
            }

            double pLive = s.LiveProbability;
            double pBlank = 1.0 - pLive;

            if (s.LiveCount > 0)
            {
                var a = s.CloneWithoutItem(ItemType.Beer);
                a.LiveCount = Math.Max(0, a.LiveCount - 1);
                a.KnownShell = null;
                result.Add(new Outcome(a, pLive));
            }

            if (s.BlankCount > 0)
            {
                var b = s.CloneWithoutItem(ItemType.Beer);
                b.BlankCount = Math.Max(0, b.BlankCount - 1);
                b.KnownShell = null;
                result.Add(new Outcome(b, pBlank));
            }

            return NormalizeOutcomes(result);
        }

        private List<Outcome> ApplyShot(AIState s, bool shootSelf)
        {
            if (s.TotalShells <= 0)
                return new List<Outcome> { new Outcome(s.Clone(), 1.0) };

            if (s.KnownShell != null)
            {
                return new List<Outcome>
                {
                    new Outcome(ResolveShot(s.Clone(), s.KnownShell.Value, shootSelf), 1.0)
                };
            }

            var outcomes = new List<Outcome>();
            double pLive = s.LiveProbability;
            double pBlank = 1.0 - pLive;

            if (s.LiveCount > 0)
            {
                var a = s.Clone();
                outcomes.Add(new Outcome(ResolveShot(a, ShellType.Live, shootSelf), pLive));
            }

            if (s.BlankCount > 0)
            {
                var b = s.Clone();
                outcomes.Add(new Outcome(ResolveShot(b, ShellType.Blank, shootSelf), pBlank));
            }

            return NormalizeOutcomes(outcomes);
        }

        private AIState ResolveShot(AIState s, ShellType shell, bool shootSelf)
        {
            if (shell == ShellType.Live)
                s.LiveCount = Math.Max(0, s.LiveCount - 1);
            else
                s.BlankCount = Math.Max(0, s.BlankCount - 1);

            s.KnownShell = null;

            bool shooterIsAi = s.AiTurn;
            int damage = Math.Max(1, s.DamageMultiplier);

            if (shell == ShellType.Live)
            {
                if (shootSelf)
                {
                    if (shooterIsAi) s.AiHp = Math.Max(0, s.AiHp - damage);
                    else s.PlayerHp = Math.Max(0, s.PlayerHp - damage);
                }
                else
                {
                    if (shooterIsAi) s.PlayerHp = Math.Max(0, s.PlayerHp - damage);
                    else s.AiHp = Math.Max(0, s.AiHp - damage);
                }

                s.DamageMultiplier = 1;
                SwitchTurnWithCuffLogic(s);
            }
            else
            {
                s.DamageMultiplier = 1;

                // Theo luật game:
                // - Bắn mình trúng đạn lép -> giữ lượt
                // - Bắn đối thủ trúng đạn lép -> đổi lượt
                if (!shootSelf)
                    SwitchTurnWithCuffLogic(s);
            }

            return s;
        }

        private void SwitchTurnWithCuffLogic(AIState s)
        {
            bool nextAiTurn = !s.AiTurn;

            // Nếu người sắp tới lượt đang bị còng thì mất lượt đó.
            if (!nextAiTurn && s.PlayerCuffed)
            {
                s.PlayerCuffed = false;
                nextAiTurn = true;
            }
            else if (nextAiTurn && s.AiCuffed)
            {
                s.AiCuffed = false;
                nextAiTurn = false;
            }

            s.AiTurn = nextAiTurn;
        }

        private List<Outcome> NormalizeOutcomes(List<Outcome> outcomes)
        {
            double sum = outcomes.Sum(x => x.Probability);

            if (sum <= 0)
                return new List<Outcome>();

            foreach (var item in outcomes)
                item.Probability /= sum;

            return outcomes;
        }

        // ============================================================
        // SNAPSHOT TRẠNG THÁI PHỤC VỤ TÌM KIẾM
        //
        // Đây chính là "biểu diễn vấn đề trong không gian trạng thái":
        // một trạng thái cần đủ thông tin để AI mô phỏng tiếp:
        // - Máu hai bên
        // - Số đạn thật / lép còn lại
        // - Viên hiện tại đã biết chưa
        // - Hệ số sát thương
        // - Túi đồ hai bên
        // - Ai đang có lượt
        // - Trạng thái còng tay
        // ============================================================
        private class AIState
        {
            public int AiHp;
            public int AiMaxHp;
            public int PlayerHp;
            public int PlayerMaxHp;

            public int LiveCount;
            public int BlankCount;

            public ShellType? KnownShell;
            public int DamageMultiplier;

            public bool AiTurn;
            public bool AiCuffed;
            public bool PlayerCuffed;

            public List<ItemType> AiInventory;
            public List<ItemType> PlayerInventory;

            public int TotalShells => LiveCount + BlankCount;

            public double LiveProbability
            {
                get
                {
                    int total = TotalShells;
                    return total > 0 ? (double)LiveCount / total : 0.0;
                }
            }

            public AIState Clone()
            {
                return new AIState
                {
                    AiHp = AiHp,
                    AiMaxHp = AiMaxHp,
                    PlayerHp = PlayerHp,
                    PlayerMaxHp = PlayerMaxHp,
                    LiveCount = LiveCount,
                    BlankCount = BlankCount,
                    KnownShell = KnownShell,
                    DamageMultiplier = DamageMultiplier,
                    AiTurn = AiTurn,
                    AiCuffed = AiCuffed,
                    PlayerCuffed = PlayerCuffed,
                    AiInventory = new List<ItemType>(AiInventory),
                    PlayerInventory = new List<ItemType>(PlayerInventory)
                };
            }

            public AIState CloneWithoutItem(ItemType item)
            {
                var copy = Clone();

                if (copy.AiTurn)
                    copy.AiInventory.Remove(item);
                else
                    copy.PlayerInventory.Remove(item);

                return copy;
            }

            public static AIState FromGameState(GameState state)
            {
                return new AIState
                {
                    AiHp = state.Player2.Hp,
                    AiMaxHp = state.Player2.MaxHp,
                    PlayerHp = state.Player1.Hp,
                    PlayerMaxHp = state.Player1.MaxHp,
                    LiveCount = state.LiveCount,
                    BlankCount = state.BlankCount,
                    KnownShell = state.KnownShell,
                    DamageMultiplier = state.DamageMultiplier,
                    AiTurn = state.CurrentPlayer == state.Player2,
                    AiCuffed = state.Player2.IsHandcuffed,
                    PlayerCuffed = state.Player1.IsHandcuffed,
                    AiInventory = state.Player2.Inventory.ToList(),
                    PlayerInventory = state.Player1.Inventory.ToList()
                };
            }
        }

        private class Outcome
        {
            public AIState State { get; set; }
            public double Probability { get; set; }

            public Outcome(AIState state, double probability)
            {
                State = state;
                Probability = probability;
            }
        }
    }
}