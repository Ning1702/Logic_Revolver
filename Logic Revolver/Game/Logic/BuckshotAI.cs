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
        public string GetBestMove(GameState state)
        {
            // [Chương 1 - Biểu diễn không gian trạng thái]
            // Mỗi trạng thái hiện tại của AI được xem như một "state" gồm:
            // HP 2 bên, số đạn thật/đạn rỗng còn lại, vật phẩm đang có, trạng thái còng tay,
            // và thông tin viên đạn hiện tại đã biết hay chưa.
            var ai = state.Player2;
            var player = state.Player1;
            int live = state.LiveCount;
            int blank = state.BlankCount;
            int total = live + blank;
            double pLive = GetPLive(state);

            if (total == 0) return "WAIT";

            // --- GIAI ĐOẠN 1: THU THẬP THÔNG TIN & HỒI PHỤC ---

            // 1. Soi đạn (Glass): Ưu tiên số 1 nếu chưa biết gì
            // Bản chuẩn cơ chế hơn: chỉ cần chưa biết viên hiện tại là có giá trị, kể cả còn 1 viên.
            // [Thuật toán áp dụng: Heuristic tham lam / Greedy]
            // Chọn ngay hành động cho lợi ích thông tin lớn nhất ở hiện tại, không cần tìm kiếm sâu.
            if (ai.Inventory.Contains(ItemType.Glass) && state.KnownShell == null)
                return "USE_GLASS";

            // 2. Hồi máu (Thuốc lá): Luôn hồi nếu mất máu
            // Bản chuẩn cơ chế hơn: ưu tiên mạnh hơn khi máu thấp để tránh chết bởi phát kế tiếp.
            // [Thuật toán áp dụng: Luật ưu tiên / Rule-based decision]
            // Đây là một luật cứng vì hồi máu là nhu cầu sinh tồn tức thời, không cần so toàn bộ cây trạng thái.
            if (ai.Inventory.Contains(ItemType.Cigarette) && ai.Hp < ai.MaxHp && (ai.Hp <= 2 || pLive >= 0.6))
                return "USE_CIGARETTE";

            // --- GIAI ĐOẠN 2: XỬ LÝ KHI ĐÃ BIẾT KẾT QUẢ (SOI ĐƯỢC HOẶC GHI NHỚ) ---

            if (state.KnownShell != null)
            {
                if (state.KnownShell == ShellType.Live)
                {
                    // [Chương 2 - Tìm kiếm tối ưu/A* dạng đơn giản]
                    // Khi đã biết chắc viên hiện tại là đạn thật, ta không cần tìm kiếm rộng nữa,
                    // mà đi thẳng theo nhánh có lợi nhất ngay lập tức.

                    // [Chương 3 - Minimax rút gọn]
                    // Nếu bắn thật thì ưu tiên các hành động làm giảm khả năng phản công của đối thủ.

                    // [TỐI ƯU HÓA COMBO]: Nếu biết là đạn thật

                    // A. Nếu có thể kết liễu bằng Dao trước rồi bắn thì ưu tiên
                    // [Thuật toán áp dụng: Heuristic kết liễu / Finishing heuristic]
                    // Khi phát hiện trạng thái thắng gần kề, AI ưu tiên nước đi kết thúc ván.
                    if (ai.Inventory.Contains(ItemType.Knife) && state.DamageMultiplier == 1 && player.Hp <= 2)
                        return "USE_KNIFE";

                    // B. Nếu có Còng tay và địch chưa bị còng -> KHÓA NÓ LẠI ĐỂ BẮN 2 PHÁT
                    // [Thuật toán áp dụng: Greedy chiến thuật]
                    // Chọn hành động tăng quyền kiểm soát lượt khi đã biết chắc viên hiện tại có lợi.
                    if (ai.Inventory.Contains(ItemType.Handcuffs) && !player.IsHandcuffed)
                        return "USE_HANDCUFFS";

                    // C. Nếu có Dao và chưa dùng -> Dùng để X2 Sát thương
                    // [Thuật toán áp dụng: Tối ưu cục bộ / Local optimization]
                    // Tăng giá trị viên đạn hiện tại trước khi khai hỏa.
                    if (ai.Inventory.Contains(ItemType.Knife) && state.DamageMultiplier == 1)
                        return "USE_KNIFE";

                    // D. Nếu máu quá thấp và chưa thể kết liễu ngay, có thể dùng Bia để đẩy viên thật nguy hiểm đi.
                    // [Thuật toán áp dụng: Heuristic phòng thủ]
                    // Nếu trạng thái hiện tại quá rủi ro cho AI, ưu tiên giảm nguy cơ trước thay vì ép damage.
                    if (ai.Inventory.Contains(ItemType.Beer) && ai.Hp == 1 && player.Hp > state.DamageMultiplier)
                        return "USE_BEER";

                    // E. Mọi thứ đã sẵn sàng -> BẮN
                    // [Thuật toán áp dụng: Quyết định tất định / Deterministic action]
                    // Vì đã biết chắc là đạn thật nên không cần suy đoán xác suất nữa.
                    return "SHOOT_OPPONENT";
                }
                else // Biết chắc chắn là ĐẠN RỖNG
                {
                    // [Chương 2 - Hàm đánh giá / quyết định đơn giản]
                    // Với đạn rỗng, cưa/x2 damage không còn ý nghĩa vì không gây sát thương.
                    // Theo đúng cơ chế Buckshot đơn giản, bắn mình là nước đi chuẩn để tua viên rỗng an toàn.

                    // [Thuật toán áp dụng: Greedy theo luật chắc chắn]
                    // Khi thông tin đã chắc chắn, chọn ngay nước có lợi tuyệt đối.
                    return "SHOOT_SELF";
                }
            }

            // --- GIAI ĐOẠN 3: XỬ LÝ KHI MÙ TỊT (DỰA VÀO XÁC SUẤT) ---

            // 3. Bia (Beer): Dùng để tua đạn nếu tỉ lệ Đạn Thật cao và AI đang ở thế nguy hiểm.
            // Chuẩn cơ chế hơn: bia hữu ích nhất khi muốn loại bỏ nguy cơ viên hiện tại.
            // [Thuật toán áp dụng: Ra quyết định theo xác suất]
            // Nếu xác suất đạn thật đủ cao thì loại bỏ viên hiện tại là hợp lý hơn việc liều bắn.
            if (ai.Inventory.Contains(ItemType.Beer) && (pLive >= 0.7 || (ai.Hp == 1 && pLive >= 0.5)))
                return "USE_BEER";

            // 4. Còng tay (Handcuffs): Dùng khi tỉ lệ Nổ cao để ép sân
            // [Thuật toán áp dụng: Heuristic kiểm soát đối thủ]
            // Không dùng quá sớm; chỉ dùng khi xác suất lợi thế đủ lớn.
            if (ai.Inventory.Contains(ItemType.Handcuffs) && !player.IsHandcuffed && pLive >= 0.65)
                return "USE_HANDCUFFS";

            // 5. Dao (Knife):
            // Chỉ liều dùng dao khi chưa soi NẾU tỉ lệ nổ rất cao và đòn đó có ý nghĩa kết liễu / tạo áp lực lớn.
            // [Thuật toán áp dụng: Heuristic rủi ro - phần thưởng]
            // Dao chỉ đáng dùng khi lợi ích kỳ vọng cao hơn rủi ro lãng phí vật phẩm.
            if (ai.Inventory.Contains(ItemType.Knife) && state.DamageMultiplier == 1 && pLive >= 0.75 && player.Hp <= 2)
                return "USE_KNIFE";

            // --- GIAI ĐOẠN 4: QUYẾT ĐỊNH BẰNG TÌM KIẾM ĐƠN GIẢN ---

            // [Chương 1 - Tìm kiếm kinh nghiệm / Best-First]
            // Thay vì chỉ xét 1 ngưỡng pLive >= 0.5, ta tạo tập hành động hợp lệ rồi chấm điểm.
            // AI sẽ chọn trạng thái/hành động "tốt nhất đầu tiên" theo hàm đánh giá.
            var candidateMoves = GetCandidateMoves(state);

            string bestMove = "SHOOT_SELF";
            double bestScore = double.NegativeInfinity;

            foreach (var move in candidateMoves)
            {
                // [Thuật toán áp dụng: Best-First Search dạng đơn giản]
                // Mỗi hành động được chấm điểm riêng, hành động điểm cao nhất sẽ được chọn.
                double score = EvaluateMove(state, move);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private List<string> GetCandidateMoves(GameState state)
        {
            var ai = state.Player2;
            var player = state.Player1;
            var moves = new List<string>();

            // [Chương 1 - Tập toán tử]
            // Đây là tập các "toán tử" hợp lệ có thể áp dụng từ trạng thái hiện tại.
            // [Thuật toán áp dụng: Sinh tập hành động / Action generation]
            // Bước này tương đương liệt kê các cạnh có thể đi ra từ state hiện tại.
            if (ai.Inventory.Contains(ItemType.Glass) && state.KnownShell == null) moves.Add("USE_GLASS");
            if (ai.Inventory.Contains(ItemType.Cigarette) && ai.Hp < ai.MaxHp) moves.Add("USE_CIGARETTE");
            if (ai.Inventory.Contains(ItemType.Beer)) moves.Add("USE_BEER");
            if (ai.Inventory.Contains(ItemType.Handcuffs) && !player.IsHandcuffed) moves.Add("USE_HANDCUFFS");
            if (ai.Inventory.Contains(ItemType.Knife) && state.DamageMultiplier == 1) moves.Add("USE_KNIFE");

            moves.Add("SHOOT_OPPONENT");
            moves.Add("SHOOT_SELF");

            return moves;
        }

        private double EvaluateMove(GameState state, string move)
        {
            var nextState = SimulateMove(state, move);
            var ai = nextState.Player2;
            var player = nextState.Player1;
            double pLive = GetPLive(state);

            bool knownLive = state.KnownShell == ShellType.Live;
            bool knownBlank = state.KnownShell == ShellType.Blank;

            // [Chương 2 - Hàm đánh giá]
            // Dạng đơn giản của f(u) = g(u) + h(u)
            // g(u): lợi ích tức thời của hành động hiện tại.
            // h(u): lợi ích ước lượng cho thế trận sau đó.
            double immediateScore = 0;
            double futureScore = 0;

            switch (move)
            {
                case "USE_GLASS":
                    // [Thuật toán áp dụng: Heuristic thông tin]
                    // Giá trị của hành động nằm ở việc giảm bất định.
                    immediateScore += state.KnownShell == null ? 6.0 : -3.0;
                    futureScore += 2.0;
                    break;

                case "USE_CIGARETTE":
                    // [Thuật toán áp dụng: Heuristic sinh tồn]
                    // Máu thấp thì điểm hồi phục cao hơn.
                    immediateScore += ai.Hp > state.Player2.Hp ? 5.0 + (state.Player2.MaxHp - state.Player2.Hp) * 1.5 : -4.0;
                    if (state.Player2.Hp == 1) immediateScore += 3.0;
                    break;

                case "USE_BEER":
                    // [Thuật toán áp dụng: Ra quyết định theo xác suất]
                    // Dùng bia để loại bỏ shell hiện tại khi shell đó có nguy cơ cao.
                    if (knownLive)
                    {
                        // Biết chắc là đạn thật -> chỉ có giá trị khi máu thấp và chưa kết liễu được đối thủ.
                        if (state.Player2.Hp == 1 && state.Player1.Hp > state.DamageMultiplier)
                            immediateScore += 3.0;
                        else
                            immediateScore -= 2.0;
                    }
                    else if (knownBlank)
                    {
                        // Biết chắc là đạn rỗng -> thường không nên phí bia, vì bắn mình còn lời hơn.
                        immediateScore -= 4.0;
                    }
                    else
                    {
                        immediateScore += pLive >= 0.7 ? 2.5 : -1.0;
                        immediateScore += state.Player2.Hp == 1 ? 1.5 : 0.0;
                    }
                    futureScore += 1.0;
                    break;

                case "USE_HANDCUFFS":
                    // [Thuật toán áp dụng: Minimax rút gọn]
                    // Còng tay làm giảm khả năng phản công ở nước tốt nhất của đối thủ.
                    immediateScore += !state.Player1.IsHandcuffed ? 5.0 : -3.0;
                    if (knownLive) immediateScore += 3.0;
                    else if (!knownBlank && pLive >= 0.65) immediateScore += 1.5;
                    futureScore += 2.0;
                    break;

                case "USE_KNIFE":
                    // [Thuật toán áp dụng: Tối ưu cục bộ]
                    // Tăng damage cho nước bắn kế tiếp nếu kỳ vọng có lợi.
                    if (state.DamageMultiplier > 1)
                    {
                        immediateScore -= 4.0;
                    }
                    else if (knownBlank)
                    {
                        // Đạn rỗng thì cưa gần như vô ích.
                        immediateScore -= 5.0;
                    }
                    else if (knownLive)
                    {
                        immediateScore += 7.0;
                        if (state.Player1.Hp <= 2) immediateScore += 4.0;
                    }
                    else
                    {
                        immediateScore += (pLive >= 0.75 && state.Player1.Hp <= 2) ? 3.0 : -1.5;
                    }
                    futureScore += 1.5;
                    break;

                case "SHOOT_OPPONENT":
                    {
                        // [Thuật toán áp dụng: Utility-based decision]
                        // Đánh giá hành động theo sát thương kỳ vọng gây ra cho đối thủ.
                        double expectedDamage = GetExpectedDamageToOpponent(state, pLive);
                        immediateScore += expectedDamage * 8.0;

                        // Thưởng lớn nếu có khả năng kết thúc đối thủ.
                        if (expectedDamage >= state.Player1.Hp)
                            immediateScore += 12.0;

                        // Nếu đã biết đạn rỗng thì bắn địch thường làm mất cơ hội giữ lượt.
                        if (knownBlank)
                            immediateScore -= 6.0;

                        // Nếu xác suất đạn rỗng cao thì bắn địch kém hiệu quả hơn.
                        immediateScore -= (1.0 - pLive) * 3.5;
                    }
                    break;

                case "SHOOT_SELF":
                    {
                        // [Chương 1 - Tìm kiếm theo kinh nghiệm]
                        // Khi pLive thấp, bắn mình là một nước "cược" hợp lý để giữ lượt.

                        // [Thuật toán áp dụng: Ra quyết định dưới bất định]
                        // AI so sánh lợi ích giữ lượt với rủi ro tự ăn damage.
                        if (knownBlank)
                        {
                            immediateScore += 9.0;
                            futureScore += 3.0;
                        }
                        else if (knownLive)
                        {
                            immediateScore -= 12.0 * state.DamageMultiplier;
                        }
                        else
                        {
                            immediateScore += (1.0 - pLive) * 8.0;
                            immediateScore -= pLive * 10.0 * state.DamageMultiplier;

                            // Chuẩn cơ chế hơn: bắn mình chỉ đẹp khi xác suất rỗng cao.
                            if (pLive <= 0.35) futureScore += 2.0;
                        }
                    }
                    break;
            }

            // [Chương 3 - Minimax rút gọn 1 lớp]
            // Ta không mô phỏng toàn bộ cây trò chơi để giữ mức đơn giản nhất.
            // Thay vào đó, ta ước lượng nhanh khả năng phản công tốt nhất của đối thủ
            // rồi trừ nó khỏi điểm của hành động hiện tại.
            double opponentBestResponse = EstimateOpponentBestResponse(nextState, move);

            // [Thuật toán áp dụng: Hàm utility tổng hợp]
            // Tổng điểm = lợi ích tức thời + lợi ích tương lai - nguy cơ phản công.
            return immediateScore + futureScore - opponentBestResponse;
        }

        private double EstimateOpponentBestResponse(GameState state, string myMove)
        {
            var ai = state.Player2;
            var player = state.Player1;
            double pLive = GetPLive(state);
            double threat = 0;

            // Đối thủ càng nhiều đồ, còn nhiều máu, và xác suất đạn thật càng cao thì áp lực càng lớn.
            // [Thuật toán áp dụng: Minimax rút gọn / One-ply adversarial estimate]
            // Không duyệt toàn bộ nước của đối thủ, chỉ ước lượng mức đe dọa tổng quát.
            threat += player.Inventory.Count * 0.6;
            threat += Math.Max(0, player.Hp - 1) * 0.5;
            threat += pLive * state.DamageMultiplier * 4.0;

            // [Chương 3 - Minimax]
            // Một số hành động của ta làm giảm "nước đi tốt nhất" của đối thủ.
            if (player.IsHandcuffed)
                threat -= 5.0;

            if (myMove == "USE_CIGARETTE")
                threat -= 1.5;

            if (myMove == "USE_BEER")
                threat -= 1.0;

            if (myMove == "SHOOT_OPPONENT")
                threat -= 2.0;

            if (myMove == "SHOOT_SELF")
            {
                // Bắn mình chỉ thật sự tốt khi có khả năng giữ lượt nhờ đạn rỗng.
                threat -= (1.0 - pLive) * 2.5;
                threat += pLive * 3.5;
            }

            // Nếu hành động của ta có khả năng giữ lượt, đối thủ khó có cơ hội phản công ngay.
            // [Thuật toán áp dụng: Heuristic thế lượt]
            if (DoesLikelyKeepTurn(state, myMove, pLive))
                threat -= 2.5;

            return Math.Max(0, threat);
        }

        private double GetExpectedDamageToOpponent(GameState state, double pLive)
        {
            // [Thuật toán áp dụng: Kỳ vọng toán học]
            // Nếu biết chắc shell thì expected damage là xác định;
            // nếu chưa biết thì dùng xác suất live để tính damage kỳ vọng.
            if (state.KnownShell == ShellType.Live)
                return state.DamageMultiplier;

            if (state.KnownShell == ShellType.Blank)
                return 0;

            return pLive * state.DamageMultiplier;
        }

        private bool DoesLikelyKeepTurn(GameState state, string move, double pLive)
        {
            // [Thuật toán áp dụng: Heuristic kiểm soát lượt]
            // Đây là một xấp xỉ đơn giản để phản ánh việc sau nước đi này AI còn chủ động hay không.
            if (move == "USE_GLASS" || move == "USE_CIGARETTE" || move == "USE_BEER" || move == "USE_HANDCUFFS" || move == "USE_KNIFE")
                return true;

            if (move == "SHOOT_SELF")
            {
                if (state.KnownShell == ShellType.Blank) return true;
                if (state.KnownShell == ShellType.Live) return false;
                return pLive < 0.5;
            }

            return false;
        }

        private double GetPLive(GameState state)
        {
            // [Thuật toán áp dụng: Suy luận xác suất]
            // Đây là xác suất đơn giản nhất của viên hiện tại là đạn thật.
            int total = state.LiveCount + state.BlankCount;
            if (total <= 0) return 0;

            if (state.KnownShell == ShellType.Live) return 1.0;
            if (state.KnownShell == ShellType.Blank) return 0.0;

            return (double)state.LiveCount / total;
        }

        private GameState SimulateMove(GameState state, string move)
        {
            // [Thuật toán áp dụng: Mô phỏng trạng thái / State transition simulation]
            // Đây là phần bổ sung để AI không chấm điểm "ảo" trên state cũ,
            // mà đánh giá sơ bộ state sau khi hành động xảy ra.
            var next = CloneState(state);
            var ai = next.Player2;
            var player = next.Player1;

            switch (move)
            {
                case "USE_GLASS":
                    // Soi đạn chủ yếu làm thay đổi thông tin, còn logic reveal thật sự
                    // có thể do game engine xử lý ở nơi khác.
                    break;

                case "USE_CIGARETTE":
                    if (ai.Inventory.Contains(ItemType.Cigarette) && ai.Hp < ai.MaxHp)
                    {
                        ai.Hp++;
                        if (ai.Hp > ai.MaxHp) ai.Hp = ai.MaxHp;
                        ai.Inventory.Remove(ItemType.Cigarette);
                    }
                    break;

                case "USE_BEER":
                    if (ai.Inventory.Contains(ItemType.Beer))
                    {
                        RemoveCurrentShell(next);
                        ai.Inventory.Remove(ItemType.Beer);
                        next.KnownShell = null;
                    }
                    break;

                case "USE_HANDCUFFS":
                    if (ai.Inventory.Contains(ItemType.Handcuffs) && !player.IsHandcuffed)
                    {
                        player.IsHandcuffed = true;
                        ai.Inventory.Remove(ItemType.Handcuffs);
                    }
                    break;

                case "USE_KNIFE":
                    if (ai.Inventory.Contains(ItemType.Knife) && next.DamageMultiplier == 1)
                    {
                        next.DamageMultiplier = 2;
                        ai.Inventory.Remove(ItemType.Knife);
                    }
                    break;

                case "SHOOT_OPPONENT":
                    ApplyShot(next, true);
                    break;

                case "SHOOT_SELF":
                    ApplyShot(next, false);
                    break;
            }

            return next;
        }

        private void ApplyShot(GameState state, bool shootOpponent)
        {
            // [Thuật toán áp dụng: Hàm chuyển trạng thái]
            // Sau khi bắn: cập nhật HP, số đạn, reset multiplier và thông tin shell.
            var ai = state.Player2;
            var player = state.Player1;

            bool isLive = state.KnownShell == ShellType.Live;
            bool isBlank = state.KnownShell == ShellType.Blank;

            if (state.KnownShell == null)
            {
                // [Thuật toán áp dụng: Xấp xỉ xác suất đơn giản]
                // Để giữ code dễ hiểu, nếu chưa biết shell thì xấp xỉ theo loại đạn đang chiếm đa số.
                isLive = state.LiveCount >= state.BlankCount && state.LiveCount > 0;
                isBlank = !isLive;
            }

            if (isLive)
            {
                if (shootOpponent)
                    player.Hp -= state.DamageMultiplier;
                else
                    ai.Hp -= state.DamageMultiplier;

                if (state.LiveCount > 0) state.LiveCount--;
            }
            else if (isBlank)
            {
                if (state.BlankCount > 0) state.BlankCount--;
            }

            state.DamageMultiplier = 1;
            state.KnownShell = null;
        }

        private void RemoveCurrentShell(GameState state)
        {
            // [Thuật toán áp dụng: State transition + xấp xỉ]
            // Loại bỏ viên hiện tại khỏi ổ đạn sau khi dùng bia.
            if (state.KnownShell == ShellType.Live)
            {
                if (state.LiveCount > 0) state.LiveCount--;
            }
            else if (state.KnownShell == ShellType.Blank)
            {
                if (state.BlankCount > 0) state.BlankCount--;
            }
            else
            {
                // Nếu chưa biết thì dùng xấp xỉ đơn giản.
                if (state.LiveCount >= state.BlankCount && state.LiveCount > 0)
                    state.LiveCount--;
                else if (state.BlankCount > 0)
                    state.BlankCount--;
            }
        }

        private GameState CloneState(GameState state)
        {
            // [Thuật toán áp dụng: Sao chép trạng thái / State cloning]
            // Tạo một bản sao để mô phỏng mà không phá hỏng state thật của game.
            return new GameState
            {
                LiveCount = state.LiveCount,
                BlankCount = state.BlankCount,
                KnownShell = state.KnownShell,
                DamageMultiplier = state.DamageMultiplier,
                Player1 = new Player(state.Player1.Name, state.Player1.MaxHp, state.Player1.IsAi)
                {
                    Hp = state.Player1.Hp,
                    IsHandcuffed = state.Player1.IsHandcuffed,
                    Inventory = new List<ItemType>(state.Player1.Inventory)
                },
                Player2 = new Player(state.Player2.Name, state.Player2.MaxHp, state.Player2.IsAi)
                {
                    Hp = state.Player2.Hp,
                    IsHandcuffed = state.Player2.IsHandcuffed,
                    Inventory = new List<ItemType>(state.Player2.Inventory)
                }
            };
        }
    }
}