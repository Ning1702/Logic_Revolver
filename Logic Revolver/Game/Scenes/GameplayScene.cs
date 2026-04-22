using Logic_Revolver.Core;
using Logic_Revolver.Engine;
using Logic_Revolver.Game.Logic;
using Logic_Revolver.Game.UI;
using Logic_Revolver.Properties;
using LogicRevolver;
using LogicRevolver.Game.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Logic_Revolver.Game.Scenes
{
    public class GameplayScene : Scene
    {
        private GameManager game;
        private BuckshotAI ai;
        private bool isProcessingAi = false;
        private Panel _view;

        private bool isPaused = false;
        private bool isPreparing = false;
        private bool isReloading = false;
        private bool isPlayerShooting = false;
        private bool isGameEnded = false;
        private bool isPlayerLostRoundTransition = false;
        private Player lastPlayer = null;

        // --- 2 BIẾN MỚI ĐỂ QUẢN LÝ TRẠNG THÁI CÒNG TAY ---
        private bool isDealerCuffed = false;
        private bool isPlayerCuffed = false;

        private int currentRound = 0;

        private Panel pnlPauseMenu;
        private Panel pnlEndGameOverlay;
        private PictureBox pbBackground;
        private PictureBox pbSetting;
        private CowboyLog rtbGameLog;
        private CowboyButton btnShootSelf, btnShootOpponent, btnItems, btnAi;

        // --- BIẾN MỚI ĐỂ AI CHƠI HỘ ĐÚNG 1 ROUND ---
        private bool _aiAutoPlayThisRound = false;
        private int _aiAutoPlayRound = 0;

        private Label lblShellInfo;
        private Label lblP1Hp, lblP2Hp;

        private FlowLayoutPanel flpP1Lives, flpP2Lives;
        private FlowLayoutPanel flpPlayerInventory, flpP2Inventory;
        private FlowLayoutPanel flpAmmo;

        private void EnableDoubleBuffer(Control ctrl)
        {
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(ctrl, true, null);
        }

        // --- HÀM HỖ TRỢ HIỂN THỊ NỀN NORMAL DỰA VÀO CÒNG TAY ---
        private void SetNormalBackground()
        {
            if (isDealerCuffed) pbBackground.Image = Resources.Normal_Dealer_curved;
            else if (isPlayerCuffed) pbBackground.Image = Resources.Normal_Player_curved;
            else pbBackground.Image = Resources.Normal_1;
        }

        // --- HÀM ĐỒNG BỘ TRẠNG THÁI CÒNG TAY TỪ GAMESTATE SANG BIẾN HIỂN THỊ ---
        private void SyncCuffedStateFromGame()
        {
            if (game == null || game.State == null || game.State.Player1 == null || game.State.Player2 == null) return;

            isPlayerCuffed = game.State.Player1.IsHandcuffed;
            isDealerCuffed = game.State.Player2.IsHandcuffed;
        }

        public override void Load()
        {
            game = new GameManager();
            ai = new BuckshotAI();

            game.OnLog += (msg) => {
                if (_view != null && rtbGameLog != null)
                {
                    if (_view.IsHandleCreated)
                    {
                        _view.Invoke((MethodInvoker)(async () => {
                            rtbGameLog.AppendText(msg + "\n");
                            rtbGameLog.ScrollToCaret();

                            string upperMsg = msg.ToUpper();

                            if (upperMsg.Contains("BẠN THUA HIỆP NÀY"))
                                isPlayerLostRoundTransition = true;

                            if (upperMsg.Contains("BẮT ĐẦU GAME: ROUND 1") || upperMsg.Contains("BẮT ĐẦU ROUND 1"))
                            {
                                if (currentRound != 1)
                                {
                                    // Nếu AI chỉ được nhờ chơi hộ 1 round thì sang round mới phải trả quyền lại
                                    if (_aiAutoPlayThisRound && _aiAutoPlayRound != 1) StopAutoPlayRound();

                                    currentRound = 1;
                                    isDealerCuffed = false; isPlayerCuffed = false; // Reset còng đầu round
                                    AudioManager.PlayBGM(Resources.bgm_round1);
                                    await RunPrepPhase();
                                }
                            }
                            else if (upperMsg.Contains("BẮT ĐẦU GAME: ROUND 2") || upperMsg.Contains("BẮT ĐẦU ROUND 2"))
                            {
                                if (currentRound != 2)
                                {
                                    // Nếu AI chỉ được nhờ chơi hộ 1 round thì sang round mới phải trả quyền lại
                                    if (_aiAutoPlayThisRound && _aiAutoPlayRound != 2) StopAutoPlayRound();

                                    currentRound = 2;
                                    isDealerCuffed = false; isPlayerCuffed = false;

                                    if (isPlayerLostRoundTransition)
                                    {
                                        AudioManager.StopAll();
                                        await AnimatePlayerDeathAndRevive();
                                        isPlayerLostRoundTransition = false;

                                        await Task.Delay(2000);
                                        AudioManager.PlayBGM(Resources.bgm_round2);
                                        await RunPrepPhase();
                                    }
                                    else
                                    {
                                        AudioManager.PlayBGM(Resources.bgm_round2);
                                        await RunPrepPhase();
                                    }
                                }
                            }
                            else if (upperMsg.Contains("BẮT ĐẦU GAME: ROUND 3") || upperMsg.Contains("BẮT ĐẦU ROUND 3"))
                            {
                                if (currentRound != 3)
                                {
                                    // Nếu AI chỉ được nhờ chơi hộ 1 round thì sang round mới phải trả quyền lại
                                    if (_aiAutoPlayThisRound && _aiAutoPlayRound != 3) StopAutoPlayRound();

                                    currentRound = 3;
                                    isDealerCuffed = false; isPlayerCuffed = false;

                                    if (isPlayerLostRoundTransition)
                                    {
                                        AudioManager.StopAll();
                                        await AnimatePlayerDeathAndRevive();
                                        isPlayerLostRoundTransition = false;

                                        await Task.Delay(2000);
                                        AudioManager.PlayBGM(Resources.bgm_round3);
                                        rtbGameLog.Visible = false;
                                        await RunPrepPhase();
                                    }
                                    else
                                    {
                                        AudioManager.PlayBGM(Resources.bgm_round3);
                                        rtbGameLog.Visible = false;
                                        await RunPrepPhase();
                                    }
                                }
                            }

                            if (msg.Contains("ĐOÀNG")) _ = AnimateScreenShake();

                            if (msg.Contains("!!! KẾT THÚC ROUND 3: BẠN THẮNG !!!"))
                            {
                                StopAutoPlayRound();
                                _ = ShowEndGameOverlay(true);
                            }

                            if (msg.Contains("!!! KẾT THÚC ROUND 3: BOT AI THẮNG !!!"))
                            {
                                StopAutoPlayRound();
                                _ = ShowEndGameOverlay(false);
                            }
                        }));
                    }
                }
            };

            game.OnGameUpdate += () => {
                if (_view != null && _view.IsHandleCreated)
                    _view.Invoke((MethodInvoker)UpdateUI);
            };

            game.OnRoundOver += async () => {
                if (isReloading || isGameEnded) return;
                isReloading = true;
                await Task.Delay(1500);

                if (_view != null && _view.IsHandleCreated)
                {
                    _view.Invoke((MethodInvoker)(async () => {
                        if (game.State.Player1.Hp > 0 && game.State.Player2.Hp > 0 && !isGameEnded && !isPlayerLostRoundTransition)
                        {
                            await AnimateReloadSequence();
                            game.LoadGun();
                            lastPlayer = null;
                        }
                        isReloading = false;
                        UpdateUI();
                    }));
                }
            };
        }

        public override void Draw(Panel view)
        {
            _view = view;
            EnableDoubleBuffer(_view);

            view.Controls.Clear();
            int w = view.Width; int h = view.Height;

            Form parentForm = view.FindForm();
            if (parentForm != null)
            {
                parentForm.KeyPreview = true;
                parentForm.KeyDown -= ParentForm_KeyDown;
                parentForm.KeyDown += ParentForm_KeyDown;
                EnableDoubleBuffer(parentForm);
            }

            pbBackground = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.StretchImage };
            SetNormalBackground(); // Dùng hàm mới
            view.Controls.Add(pbBackground);

            pbSetting = new PictureBox
            {
                Size = new Size(56, 56),
                Location = new Point(w - 70, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = Resources.Setting,
                Cursor = Cursors.Hand
            };

            pbSetting.Click += (s, e) =>
            {
                if (!isGameEnded)
                {
                    TogglePause();
                }

                if (pbSetting != null)
                    pbSetting.BringToFront();
            };

            pbBackground.Controls.Add(pbSetting);
            pbSetting.BringToFront();

            GroupBox gbBot = new CowboyGroupBox() { Text = "BOT (PLAYER 2)", Size = new Size(350, 150), Location = new Point(20, 20), BackColor = Color.FromArgb(180, 10, 10, 10), ForeColor = Color.Gold };
            lblP2Hp = new Label() { Text = "HP: 4/4", Location = new Point(10, 30), AutoSize = true, Font = new Font("Arial", 11, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent };
            flpP2Lives = new FlowLayoutPanel() { Size = new Size(150, 40), Location = new Point(70, 25), BackColor = Color.Transparent };
            flpP2Inventory = new FlowLayoutPanel() { Size = new Size(330, 60), Location = new Point(10, 70), BackColor = Color.Transparent };
            EnableDoubleBuffer(flpP2Lives); EnableDoubleBuffer(flpP2Inventory);
            gbBot.Controls.Add(lblP2Hp); gbBot.Controls.Add(flpP2Lives); gbBot.Controls.Add(flpP2Inventory); pbBackground.Controls.Add(gbBot);

            GroupBox gbPlayer = new CowboyGroupBox() { Text = "BẠN (PLAYER 1)", Size = new Size(350, 150), Location = new Point(20, h - 180), BackColor = Color.FromArgb(180, 10, 10, 10), ForeColor = Color.Gold };
            lblP1Hp = new Label() { Text = "HP: 4/4", Location = new Point(10, 30), AutoSize = true, Font = new Font("Arial", 11, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent };
            flpP1Lives = new FlowLayoutPanel() { Size = new Size(150, 40), Location = new Point(70, 25), BackColor = Color.Transparent };
            flpPlayerInventory = new FlowLayoutPanel() { Size = new Size(330, 60), Location = new Point(10, 70), BackColor = Color.Transparent };
            EnableDoubleBuffer(flpP1Lives); EnableDoubleBuffer(flpPlayerInventory);
            gbPlayer.Controls.Add(lblP1Hp); gbPlayer.Controls.Add(flpP1Lives); gbPlayer.Controls.Add(flpPlayerInventory); pbBackground.Controls.Add(gbPlayer);

            // ===== NÚT AI =====
            btnAi = new CowboyButton()
            {
                Text = "AI",
                Size = new Size(110, 42),
                Font = new Font("Georgia", 10, FontStyle.Bold),
                Location = new Point(gbPlayer.Left, gbPlayer.Top - 100)
            };
            btnAi.SetColors(Color.FromArgb(70, 40, 20));
            btnAi.Click += async (s, e) =>
            {
                if (isPreparing || isPaused || isReloading || isPlayerShooting || isGameEnded) return;

                var result = MessageBox.Show(
                    "Bạn muốn nhờ AI chơi hộ không?",
                    "AI Hỗ Trợ",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    if (pbSetting != null)
                        pbSetting.BringToFront();
                    return;
                }

                _aiAutoPlayThisRound = true;
                _aiAutoPlayRound = currentRound;

                if (btnAi != null) btnAi.Enabled = false;
                if (btnItems != null) btnItems.Enabled = false;
                if (btnShootSelf != null) btnShootSelf.Enabled = false;
                if (btnShootOpponent != null) btnShootOpponent.Enabled = false;
                if (flpPlayerInventory != null) flpPlayerInventory.Enabled = false;

                await RunAutoPlayRound();
            };
            pbBackground.Controls.Add(btnAi);
            // ===================

            // ===== NÚT ITEMS =====
            btnItems = new CowboyButton()
            {
                Text = "ITEMS",
                Size = new Size(110, 42),
                Font = new Font("Georgia", 10, FontStyle.Bold),
                Location = new Point(gbPlayer.Left, gbPlayer.Top - 50)
            };
            btnItems.SetColors(Color.FromArgb(70, 40, 20));
            btnItems.Click += (s, e) =>
            {
                using (Item frm = new Item())
                {
                    frm.StartPosition = FormStartPosition.CenterParent;
                    frm.ShowDialog(view.FindForm());
                }

                if (pbSetting != null)
                    pbSetting.BringToFront();
            };
            pbBackground.Controls.Add(btnItems);
            // =====================

            flpAmmo = new FlowLayoutPanel() { Size = new Size(200, 80), Location = new Point((w - 200) / 2 + 20, h - 80), BackColor = Color.Transparent, FlowDirection = FlowDirection.LeftToRight };
            EnableDoubleBuffer(flpAmmo);
            pbBackground.Controls.Add(flpAmmo);

            lblShellInfo = new Label() { Text = "AMMO:", AutoSize = true, Font = new Font("Georgia", 14, FontStyle.Bold), ForeColor = Color.WhiteSmoke, BackColor = Color.Transparent, Location = new Point(flpAmmo.Left - 95, flpAmmo.Top + 20) };
            pbBackground.Controls.Add(lblShellInfo);

            btnShootSelf = new CowboyButton() { Text = "BẮN MÌNH", Size = new Size(160, 54), Font = new Font("Georgia", 12, FontStyle.Bold), Visible = false };
            btnShootSelf.SetColors(Color.FromArgb(70, 40, 20)); btnShootSelf.Location = new Point((w / 2) - 290, h - 140);
            btnShootSelf.Click += async (s, e) => { if (isReloading || isPlayerShooting || isGameEnded) return; await HandlePlayerShoot(Target.Self); };

            btnShootOpponent = new CowboyButton() { Text = "BẮN ĐỊCH", Size = new Size(160, 54), Font = new Font("Georgia", 12, FontStyle.Bold), Visible = false };
            btnShootOpponent.SetColors(Color.FromArgb(139, 0, 0)); btnShootOpponent.Location = new Point((w / 2) + 128, h - 140);
            btnShootOpponent.Click += async (s, e) => { if (isReloading || isPlayerShooting || isGameEnded) return; await HandlePlayerShoot(Target.Opponent); };

            pbBackground.Controls.Add(btnShootSelf); pbBackground.Controls.Add(btnShootOpponent);
            rtbGameLog = new CowboyLog() { Size = new Size(250, h - 100), Location = new Point(w - 270, 70) };
            pbBackground.Controls.Add(rtbGameLog);

            CreatePauseMenu(view);
            game.StartGame();
            UpdateUI();
        }

        private void ParentForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && !isGameEnded) TogglePause();
        }

        private void ToggleControls(bool enable)
        {
            if (btnShootOpponent != null && btnShootOpponent.Visible != enable) btnShootOpponent.Visible = enable;
            if (btnShootSelf != null && btnShootSelf.Visible != enable) btnShootSelf.Visible = enable;
            if (flpPlayerInventory != null && flpPlayerInventory.Enabled != enable) flpPlayerInventory.Enabled = enable;
        }

        // --- HÀM MỚI: DỪNG CHẾ ĐỘ AI CHƠI HỘ VÀ TRẢ QUYỀN LẠI CHO NGƯỜI CHƠI ---
        private void StopAutoPlayRound()
        {
            _aiAutoPlayThisRound = false;
            _aiAutoPlayRound = 0;

            if (btnAi != null) btnAi.Enabled = true;
            if (btnItems != null) btnItems.Enabled = true;
        }

        // --- HÀM MỚI: AI CHỈ CHƠI HỘ ĐÚNG ROUND ĐƯỢC YÊU CẦU ---
        private async Task RunAutoPlayRound()
        {
            try
            {
                while (_aiAutoPlayThisRound && !isGameEnded)
                {
                    if (game == null || game.State == null) break;

                    // Chỉ chơi hộ đúng round đã bấm AI
                    if (currentRound != _aiAutoPlayRound) break;

                    while (isPaused || isPreparing || isReloading || isPlayerShooting || isPlayerLostRoundTransition || isGameEnded)
                        await Task.Delay(250);

                    if (isGameEnded) break;
                    if (currentRound != _aiAutoPlayRound) break;

                    // Nếu đang là lượt bot thì để bot tự chạy logic cũ
                    if (game.State.CurrentPlayer != null && game.State.CurrentPlayer.IsAi)
                    {
                        if (!isProcessingAi)
                            await RunAiTurn();
                        else
                            await Task.Delay(250);

                        continue;
                    }

                    // Nếu đang là lượt người chơi thì cho AI quyết định hộ
                    await ExecuteAutoMoveForPlayer();
                    await Task.Delay(500);
                }
            }
            finally
            {
                StopAutoPlayRound();
                UpdateUI();
            }
        }

        // --- HÀM MỚI: TẠO STATE ĐẢO VAI ĐỂ TÁI DÙNG BuckshotAI CHO PLAYER 1 ---
        private GameState FlipStateForPlayerAuto(GameState original)
        {
            var flipped = new GameState
            {
                Player1 = ClonePlayer(original.Player2),
                Player2 = ClonePlayer(original.Player1),
                LiveCount = original.LiveCount,
                BlankCount = original.BlankCount,
                KnownShell = original.KnownShell,
                DamageMultiplier = original.DamageMultiplier
            };

            // BuckshotAI luôn ra quyết định cho Player2 trong state truyền vào.
            // Vì thế khi nhờ AI chơi hộ Player1, ta đảo Player1 gốc thành Player2 mới.
            flipped.CurrentPlayer = flipped.Player2;
            return flipped;
        }

        // --- HÀM MỚI: CLONE PLAYER ĐỂ MÔ PHỎNG QUYẾT ĐỊNH MÀ KHÔNG PHÁ STATE THẬT ---
        private Player ClonePlayer(Player p)
        {
            return new Player(p.Name, p.MaxHp, p.IsAi)
            {
                Hp = p.Hp,
                IsHandcuffed = p.IsHandcuffed,
                Inventory = new List<ItemType>(p.Inventory)
            };
        }

        // --- HÀM MỚI: AI QUYẾT ĐỊNH HỘ CHO PLAYER 1 ---
        private async Task ExecuteAutoMoveForPlayer()
        {
            if (game == null || game.State == null) return;
            if (game.State.CurrentPlayer == null) return;
            if (game.State.CurrentPlayer != game.State.Player1) return;

            string move = ai.GetBestMove(FlipStateForPlayerAuto(game.State));

            if (string.IsNullOrEmpty(move)) return;

            if (move.StartsWith("USE"))
            {
                ItemType item = ItemType.Beer;
                if (move.Contains("KNIFE")) item = ItemType.Knife;
                else if (move.Contains("GLASS")) item = ItemType.Glass;
                else if (move.Contains("HANDCUFFS")) item = ItemType.Handcuffs;
                else if (move.Contains("CIGARETTE")) item = ItemType.Cigarette;

                await HandleItemUse(game.State.Player1, item);
            }
            else if (move == "SHOOT_OPPONENT")
            {
                await HandlePlayerShoot(Target.Opponent);
            }
            else if (move == "SHOOT_SELF")
            {
                await HandlePlayerShoot(Target.Self);
            }
        }

        // --- ĐÃ CẬP NHẬT ẢNH RELOAD ---
        private async Task AnimateReloadSequence()
        {
            ToggleControls(false);
            SetNormalBackground();
            await Task.Delay(400);

            if (pbBackground != null) pbBackground.Image = Resources.Dealer_Takegun_1;
            await Task.Delay(200);

            // DÙNG ẢNH RELOAD MỚI
            if (pbBackground != null) pbBackground.Image = Resources.Reload;

            AudioManager.PlaySound(Resources.sfx_reload);
            await Task.Delay(1500);

            SetNormalBackground();
            await Task.Delay(400);
        }

        private async Task RunPrepPhase()
        {
            isPreparing = true;
            await AnimateReloadSequence();
            Label lblPrep = new Label() { AutoSize = true, Font = new Font("Georgia", 72, FontStyle.Bold), ForeColor = Color.Yellow, BackColor = Color.Transparent };
            lblPrep.Parent = pbBackground; pbBackground.Controls.Add(lblPrep); lblPrep.BringToFront();

            for (int i = 5; i > 0; i--)
            {
                lblPrep.Text = i.ToString();
                lblPrep.Location = new Point((_view.Width - lblPrep.Width) / 2, (_view.Height - lblPrep.Height) / 2);
                await Task.Delay(1000);
            }
            pbBackground.Controls.Remove(lblPrep); lblPrep.Dispose();
            if (pbSetting != null) pbSetting.BringToFront();
            isPreparing = false; lastPlayer = null;
            if (_view != null && _view.IsHandleCreated) _view.Invoke((MethodInvoker)UpdateUI);
        }

        private async Task AnimatePlayerDeathAndRevive()
        {
            if (_view == null) return;
            SetNormalBackground();
            lastPlayer = null;

            Panel topLid = new Panel { BackColor = Color.Black, Width = _view.Width, Height = _view.Height / 2 + 5, Location = new Point(0, 0) };
            Panel bottomLid = new Panel { BackColor = Color.Black, Width = _view.Width, Height = _view.Height / 2 + 5, Location = new Point(0, _view.Height / 2) };
            _view.Controls.Add(topLid); _view.Controls.Add(bottomLid);
            topLid.BringToFront(); bottomLid.BringToFront();

            await Task.Delay(350);
            AudioManager.StopAll();
            await Task.Delay(2150);

            AudioManager.PlaySound(Resources.sfx_splash);
            await Task.Delay(600);
            AudioManager.PlaySound(Resources.sfx_breath);
            await Task.Delay(1500);

            int steps = 20; int stepHeight = topLid.Height / steps;
            for (int i = 0; i < steps; i++)
            {
                topLid.Height -= stepHeight; bottomLid.Top += stepHeight; bottomLid.Height -= stepHeight;
                await Task.Delay(50);
            }

            _view.Controls.Remove(topLid); _view.Controls.Remove(bottomLid);
            topLid.Dispose(); bottomLid.Dispose();

            SetNormalBackground();
            lastPlayer = null;
        }

        private async Task ShowEndGameOverlay(bool isWin)
        {
            if (_view == null || isGameEnded) return;
            isGameEnded = true;
            ToggleControls(false);
            StopAutoPlayRound();

            if (pbSetting != null) pbSetting.Visible = false;
            await Task.Delay(350);
            AudioManager.StopAll();

            pnlEndGameOverlay = new Panel
            {
                Size = _view.ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(140, 0, 0, 0)
            };

            Panel pnlCard = new Panel
            {
                Size = new Size(620, 320),
                Location = new Point((_view.Width - 620) / 2, (_view.Height - 320) / 2),
                BackColor = Color.FromArgb(210, 70, 45, 25),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblResult = new Label
            {
                AutoSize = false,
                Size = new Size(520, 80),
                Location = new Point(50, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Georgia", 30, FontStyle.Bold),
                ForeColor = isWin ? Color.Gold : Color.OrangeRed,
                BackColor = Color.Transparent,
                Text = isWin ? "WIN!" : "GAME OVER!"
            };

            Label lblSub = new Label
            {
                AutoSize = false,
                Size = new Size(520, 50),
                Location = new Point(50, 110),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Georgia", 14, FontStyle.Bold),
                ForeColor = Color.WhiteSmoke,
                BackColor = Color.Transparent,
                Text = isWin ? "Bạn đã hạ gục đối thủ." : "Bạn đã bị đánh bại."
            };

            CowboyButton btnReplay = new CowboyButton
            {
                Text = "CHƠI LẠI",
                Size = new Size(180, 52),
                Location = new Point(120, 220),
                Font = new Font("Georgia", 12, FontStyle.Bold)
            };
            btnReplay.SetColors(Color.FromArgb(101, 67, 33));
            btnReplay.Click += (s, e) =>
            {
                AudioManager.StopAll();
                SceneManager.LoadScene(new GameplayScene());
            };

            CowboyButton btnMenu = new CowboyButton
            {
                Text = "VỀ MENU",
                Size = new Size(180, 52),
                Location = new Point(320, 220),
                Font = new Font("Georgia", 12, FontStyle.Bold)
            };
            btnMenu.SetColors(Color.DarkGoldenrod);
            btnMenu.Click += (s, e) =>
            {
                AudioManager.StopAll();
                Form parent = _view.FindForm();
                if (parent != null)
                {
                    FormMenu menu = new FormMenu();
                    menu.Show();
                    parent.Close();
                }
            };

            pnlCard.Controls.Add(lblResult);
            pnlCard.Controls.Add(lblSub);
            pnlCard.Controls.Add(btnReplay);
            pnlCard.Controls.Add(btnMenu);

            pnlEndGameOverlay.Controls.Add(pnlCard);
            _view.Controls.Add(pnlEndGameOverlay);
            pnlEndGameOverlay.BringToFront();

            if (isWin) AudioManager.PlaySound(Resources.sfx_win);
        }

        private void CreatePauseMenu(Panel view)
        {
            int w = view.Width; int h = view.Height;
            pnlPauseMenu = new Panel() { Size = new Size(400, 450), Location = new Point((w - 400) / 2, (h - 450) / 2), BackColor = Color.FromArgb(101, 67, 33), BorderStyle = BorderStyle.Fixed3D, Visible = false };
            view.Controls.Add(pnlPauseMenu); pnlPauseMenu.BringToFront();
            Label lblTitle = new Label() { Text = "TẠM DỪNG\n(Nhấn ESC để thoát)", TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Georgia", 16, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point((400 - 220) / 2, 30) };
            pnlPauseMenu.Controls.Add(lblTitle);

            int btnWidth = 250; int startY = 120; int gap = 70;

            CowboyButton btnResume = new CowboyButton() { Text = "TIẾP TỤC", Size = new Size(btnWidth, 50), Location = new Point((400 - btnWidth) / 2, startY) };
            btnResume.SetColors(Color.DarkGreen);
            btnResume.Click += (s, e) => TogglePause();
            pnlPauseMenu.Controls.Add(btnResume);

            CowboyButton btnRestart = new CowboyButton() { Text = "CHƠI LẠI", Size = new Size(btnWidth, 50), Location = new Point((400 - btnWidth) / 2, startY + gap) };
            btnRestart.Click += (s, e) => { Application.Restart(); };
            pnlPauseMenu.Controls.Add(btnRestart);

            CowboyButton btnSound = new CowboyButton() { Text = "ÂM THANH", Size = new Size(btnWidth, 50), Location = new Point((400 - btnWidth) / 2, startY + gap * 2) };
            btnSound.SetColors(Color.DarkGoldenrod);
            btnSound.Click += (s, e) => {
                using (FormSound frm = new FormSound())
                {
                    frm.StartPosition = FormStartPosition.CenterParent;
                    frm.ShowDialog(view.FindForm());
                }
                pnlPauseMenu.BringToFront();
            };
            pnlPauseMenu.Controls.Add(btnSound);

            CowboyButton btnExit = new CowboyButton() { Text = "THOÁT GAME", Size = new Size(btnWidth, 50), Location = new Point((400 - btnWidth) / 2, startY + gap * 3) };
            btnExit.SetColors(Color.DarkRed);
            btnExit.Click += (s, e) => { Application.Exit(); };
            pnlPauseMenu.Controls.Add(btnExit);

            if (pbSetting != null) pbSetting.BringToFront();
        }

        private void TogglePause()
        {
            if (isGameEnded) return;
            isPaused = !isPaused; pnlPauseMenu.Visible = isPaused; pnlPauseMenu.BringToFront();
            if (btnShootOpponent != null) btnShootOpponent.Enabled = !isPaused;
            if (btnShootSelf != null) btnShootSelf.Enabled = !isPaused;
            if (flpPlayerInventory != null) flpPlayerInventory.Enabled = !isPaused;
            if (btnAi != null) btnAi.Enabled = !isPaused && !_aiAutoPlayThisRound;
            if (btnItems != null) btnItems.Enabled = !isPaused && !_aiAutoPlayThisRound;
        }

        private async void UpdateUI()
        {
            if (game.State.Player1 == null) return;
            if (isGameEnded) return;

            SyncCuffedStateFromGame();

            string p1HpTxt = $"HP: {game.State.Player1.Hp}/{game.State.Player1.MaxHp}";
            if (lblP1Hp.Text != p1HpTxt) lblP1Hp.Text = p1HpTxt;

            string p2HpTxt = $"HP: {game.State.Player2.Hp}/{game.State.Player2.MaxHp}";
            if (lblP2Hp.Text != p2HpTxt) lblP2Hp.Text = p2HpTxt;

            RenderHealth(flpP1Lives, game.State.Player1.Hp, game.State.Player1.MaxHp);
            RenderHealth(flpP2Lives, game.State.Player2.Hp, game.State.Player2.MaxHp);
            RenderInventory(flpPlayerInventory, game.State.Player1);
            RenderInventory(flpP2Inventory, game.State.Player2);
            RenderAmmo(game.State.LiveCount + game.State.BlankCount);

            if (btnAi != null) btnAi.Enabled = !isPaused && !isPreparing && !isReloading && !isPlayerShooting && !isGameEnded && !_aiAutoPlayThisRound;
            if (btnItems != null) btnItems.Enabled = !isPaused && !isPreparing && !isReloading && !isPlayerShooting && !isGameEnded && !_aiAutoPlayThisRound;

            if (isPreparing || isPaused || isReloading || isPlayerShooting || isPlayerLostRoundTransition)
            {
                ToggleControls(false);
                return;
            }

            if (game.State.CurrentPlayer.IsAi)
            {
                ToggleControls(false);
                if (lastPlayer != game.State.Player2)
                {
                    lastPlayer = game.State.Player2;
                    SetNormalBackground();
                }
                if (!isProcessingAi && game.State.Player1.Hp > 0 && game.State.Player2.Hp > 0) await RunAiTurn();
            }
            else
            {
                if (lastPlayer != game.State.Player1)
                {
                    lastPlayer = game.State.Player1;
                    if (game.State.Player1.Hp > 0) await PlayerGrabGunSequence();
                }
                else
                {
                    ToggleControls(true);
                }
            }
        }

        private void RenderHealth(FlowLayoutPanel panel, int current, int max)
        {
            while (panel.Controls.Count < max) panel.Controls.Add(new PictureBox { Size = new Size(30, 30), SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding(2), BackColor = Color.Transparent });
            while (panel.Controls.Count > max) { panel.Controls[panel.Controls.Count - 1].Dispose(); panel.Controls.RemoveAt(panel.Controls.Count - 1); }
            for (int i = 0; i < max; i++)
            {
                PictureBox pb = (PictureBox)panel.Controls[i]; string state = (i < current) ? "Full" : "Empty";
                if (pb.Tag == null || pb.Tag.ToString() != state)
                {
                    pb.Image = (state == "Full") ? Resources.Heart_Health : null;
                    pb.BorderStyle = (state == "Full") ? BorderStyle.None : BorderStyle.FixedSingle;
                    pb.Tag = state;
                }
            }
        }

        private void RenderAmmo(int count)
        {
            if (count < 0) count = 0;
            while (flpAmmo.Controls.Count < count) flpAmmo.Controls.Add(new PictureBox { Size = new Size(30, 70), SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding(5), BackColor = Color.Transparent, Image = Resources.Bullet });
            while (flpAmmo.Controls.Count > count) { Control c = flpAmmo.Controls[flpAmmo.Controls.Count - 1]; flpAmmo.Controls.Remove(c); c.Dispose(); }
        }

        private void RenderInventory(FlowLayoutPanel panel, Player p)
        {
            bool changed = (panel.Controls.Count != p.Inventory.Count);
            if (!changed) { for (int i = 0; i < p.Inventory.Count; i++) if ((ItemType)panel.Controls[i].Tag != p.Inventory[i]) { changed = true; break; } }
            if (changed)
            {
                panel.Controls.Clear();
                foreach (var item in p.Inventory)
                {
                    PictureBox pb = CreateItemIcon(item);
                    if (p == game.State.Player1)
                    {
                        pb.Cursor = Cursors.Hand;
                        pb.DoubleClick += async (s, e) => {
                            if (game.State.CurrentPlayer == game.State.Player1 && !isPreparing && !isPaused && !isReloading && !isPlayerShooting && !isGameEnded && !_aiAutoPlayThisRound)
                            {
                                await HandleItemUse(game.State.Player1, (ItemType)pb.Tag);
                            }
                        };
                    }
                    panel.Controls.Add(pb);
                }
            }
        }

        // --- ĐÃ TÍCH HỢP LOGIC GẮN CÒNG VÀ BỎ CÒNG ---
        private async Task HandleItemUse(Player p, ItemType item)
        {
            if (isPlayerShooting || isGameEnded) return;
            isPlayerShooting = true;

            try
            {
                ToggleControls(false);

                if (p == game.State.Player1) { pbBackground.Image = Resources.Player_TakeGun_1; await Task.Delay(400); }
                else { pbBackground.Image = Resources.Dealer_Takegun_1; await Task.Delay(500); }

                PlayItemSound(item);

                // Nếu người chơi dùng còng tay
                if (item == ItemType.Handcuffs)
                {
                    if (p == game.State.Player1) isDealerCuffed = true;
                    else isPlayerCuffed = true;
                }

                await Task.Delay(600); // Animation chung chờ hiệu ứng item

                game.UseItem(p, item);
                SyncCuffedStateFromGame();

                if (p == game.State.Player1) pbBackground.Image = Resources.Player_Holdgun;
                else SetNormalBackground();
            }
            finally
            {
                isPlayerShooting = false;
                UpdateUI();
            }
        }

        private PictureBox CreateItemIcon(ItemType type)
        {
            PictureBox pb = new PictureBox { Size = new Size(50, 50), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Transparent, Tag = type };
            switch (type)
            {
                case ItemType.Beer: pb.Image = Resources.Beer; break;
                case ItemType.Knife: pb.Image = Resources.Saw; break;
                case ItemType.Glass: pb.Image = Resources.Magnifier; break;
                case ItemType.Handcuffs: pb.Image = Resources.Handcuff; break;
                case ItemType.Cigarette: pb.Image = Resources.Tobacco; break;
            }
            return pb;
        }

        private void PlayItemSound(ItemType type)
        {
            switch (type)
            {
                case ItemType.Beer: AudioManager.PlaySound(Resources.sfx_item_beer); break;
                case ItemType.Knife: AudioManager.PlaySound(Resources.sfx_item_saw); break;
                case ItemType.Glass: AudioManager.PlaySound(Resources.sfx_item_glass); break;
                case ItemType.Handcuffs: AudioManager.PlaySound(Resources.sfx_item_handcuff); break;
                case ItemType.Cigarette: AudioManager.PlaySound(Resources.sfx_item_cigarette); break;
            }
        }

        // --- ĐÃ BỔ SUNG CÁC ẢNH CÒNG TAY KHI LẤY SÚNG ---
        private async Task PlayerGrabGunSequence()
        {
            ToggleControls(false);
            isPlayerCuffed = false; // Bỏ còng vì đã đến lượt tự hành động
            pbBackground.Image = isDealerCuffed ? Resources.Player_TakeGun_Dealer_curved : Resources.Player_TakeGun_1;
            await Task.Delay(250);
            pbBackground.Image = Resources.Player_Holdgun;
            ToggleControls(true);
        }

        // --- ĐÃ BỔ SUNG CÁC ẢNH CÒNG TAY KHI BỊ BẮN & ẢNH HIT_4 ---
        private async Task AnimateEnemyHit()
        {
            if (pbBackground == null) return;
            pbBackground.Image = isDealerCuffed ? Resources.Enemy_Hit_1_Dealer_curved : Resources.Enemy_Hit_1;
            await Task.Delay(80);

            pbBackground.Image = Resources.Enemy_Hit_2;
            AudioManager.PlaySound(Resources.sfx_hit);
            await Task.Delay(100);

            pbBackground.Image = Resources.Enemy_Hit_3;
            await Task.Delay(400);

            pbBackground.Image = Resources.Enemy_Hit_4; // ẢNH HIT MỚI DỤI XUỐNG BÀN
            await Task.Delay(400);

            if (game.State.Player2.Hp > 0) SetNormalBackground();
        }

        // --- HÀM MỚI: KHI NGƯỜI CHƠI BỊ TRÚNG ĐẠN THÌ PHÁT THÊM TIẾNG HIT ---
        private async Task PlayPlayerHitEffect()
        {
            AudioManager.PlaySound(Resources.sfx_hit);
            await Task.Delay(180);
        }

        // --- ĐÃ BỔ SUNG ẢNH CÒNG TAY KHI BẠN BẮN ---
        private async Task HandlePlayerShoot(Target target)
        {
            if (isPlayerShooting || isGameEnded) return;
            isPlayerShooting = true;
            try
            {
                ToggleControls(false);

                if (target == Target.Opponent)
                    pbBackground.Image = isDealerCuffed ? Resources.Player_ShootYou_Dealer_curved : Resources.Player_ShootYou;
                else
                    pbBackground.Image = isDealerCuffed ? Resources.Player_ShootSelf_Dealer_curved : Resources.Player_ShootSelf;

                AudioManager.PlaySound(Resources.sfx_cock_gun); await Task.Delay(600);

                game.Shoot(target);
                SyncCuffedStateFromGame();

                if (game.LastFiredShell == ShellType.Live)
                {
                    AudioManager.PlaySound(Resources.sfx_shoot_live);

                    if (target == Target.Opponent)
                    {
                        Task recoil = AnimateRecoil();
                        Task hit = AnimateEnemyHit();
                        await Task.WhenAll(recoil, hit);
                    }
                    else
                    {
                        await AnimateRecoil();
                        await PlayPlayerHitEffect();
                    }
                }
                else
                {
                    AudioManager.PlaySound(Resources.sfx_shoot_blank); await Task.Delay(500);
                }

                if (isPlayerLostRoundTransition)
                {
                    SetNormalBackground();
                    lastPlayer = null;
                }
                else if (game.State.CurrentPlayer == game.State.Player1 && (game.State.LiveCount + game.State.BlankCount > 0))
                {
                    pbBackground.Image = Resources.Player_Holdgun;
                }
            }
            finally { isPlayerShooting = false; UpdateUI(); }
        }

        private async Task AnimateGrabGun_Dealer()
        {
            if (pbBackground.Image == Resources.Dealer_Holdgun) return;
            isDealerCuffed = false; // Bỏ còng vì bot đã tự hành động
            pbBackground.Image = Resources.Dealer_Takegun_1;
            await Task.Delay(200);
            pbBackground.Image = Resources.Dealer_Holdgun;
            await Task.Delay(400);
        }

        private async Task AnimateRecoil() { if (pbBackground == null) return; int originalY = pbBackground.Top; for (int i = 0; i < 5; i++) { pbBackground.Top += 3; await Task.Delay(10); } for (int i = 0; i < 10; i++) { pbBackground.Top -= 1; await Task.Delay(20); } pbBackground.Top = originalY; }
        private async Task AnimateScreenShake() { Form f = _view.FindForm(); if (f == null) return; Point o = f.Location; Random r = new Random(); for (int i = 0; i < 10; i++) { f.Location = new Point(o.X + r.Next(-10, 10), o.Y + r.Next(-10, 10)); await Task.Delay(30); } f.Location = o; }

        // --- ĐÃ BỔ SUNG ẢNH CÒNG TAY KHI BOT BẮN ---
        private async Task RunAiTurn()
        {
            if (isProcessingAi || isGameEnded) return;
            isProcessingAi = true;
            while (game.State.CurrentPlayer.IsAi && game.State.Player1.Hp > 0 && game.State.Player2.Hp > 0 && !isGameEnded)
            {
                if (game.State.LiveCount + game.State.BlankCount == 0) break;
                while (isPaused || isPreparing || isReloading || isPlayerShooting || isGameEnded || isPlayerLostRoundTransition) await Task.Delay(500);
                if (isGameEnded) break;

                string move = ai.GetBestMove(game.State);
                if (move.StartsWith("SHOOT"))
                {
                    await AnimateGrabGun_Dealer(); await Task.Delay(3000);

                    if (move == "SHOOT_OPPONENT")
                        pbBackground.Image = isPlayerCuffed ? Resources.Dealer_ShootYou_Player_curved : Resources.Dealer_ShootYou;
                    else
                        pbBackground.Image = Resources.Dealer_ShootSelf;

                    AudioManager.PlaySound(Resources.sfx_cock_gun); await Task.Delay(600);

                    game.Shoot(move == "SHOOT_OPPONENT" ? Target.Opponent : Target.Self);
                    SyncCuffedStateFromGame();

                    if (game.LastFiredShell == ShellType.Live)
                    {
                        AudioManager.PlaySound(Resources.sfx_shoot_live);

                        if (move == "SHOOT_OPPONENT")
                        {
                            await AnimateRecoil();
                            await PlayPlayerHitEffect();
                        }
                        else
                        {
                            await Task.WhenAll(AnimateRecoil(), AnimateEnemyHit());
                        }
                    }
                    else
                    {
                        AudioManager.PlaySound(Resources.sfx_shoot_blank);
                        await Task.Delay(500);
                    }

                    if (isPlayerLostRoundTransition)
                    {
                        SetNormalBackground();
                        lastPlayer = null;
                    }
                    else if (game.State.Player1.Hp > 0 && game.State.CurrentPlayer == game.State.Player2 && (game.State.LiveCount + game.State.BlankCount > 0))
                    {
                        // Nếu AI vẫn còn lượt sau khi player bị còng mất lượt thì trở về nền bình thường
                        SetNormalBackground();
                    }
                }
                else if (move.StartsWith("USE"))
                {
                    ItemType item = ItemType.Beer;
                    if (move.Contains("KNIFE")) item = ItemType.Knife; else if (move.Contains("GLASS")) item = ItemType.Glass; else if (move.Contains("HANDCUFFS")) item = ItemType.Handcuffs; else if (move.Contains("CIGARETTE")) item = ItemType.Cigarette;
                    await HandleItemUse(game.State.Player2, item);
                }
                else { await Task.Delay(500); }
            }
            isProcessingAi = false;
        }

        public override void Update() { }
    }
}