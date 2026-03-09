<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GamePlay.aspx.cs" Inherits="gamethebaiv2.Game.GamePlay" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Hầm Ngục Tử Chiến - Quyết Đấu</title>
    <link href="https://fonts.googleapis.com/css2?family=Cinzel:wght@700&display=swap" rel="stylesheet">
    <style>
        :root { --hp: #ff4757; --gold: #eab543; --mana: #ff3f3f; --border: #ffd700; --panel-bg: rgba(0,0,0,0.85); }
        body, html { margin: 0; padding: 0; width: 100vw; height: 100vh; background: #000; overflow: hidden; font-family: 'Segoe UI', sans-serif; }

        /* KHÓA CỨNG BACKGROUND */
        .battle-container {
            width: 100vw; height: 100vh;
            background: url('<%= ResolveUrl("~/Images/bg_dungeon.png") %>') no-repeat center center;
            background-size: cover; position: relative;
        }

        /* THANH HUD VÀNG TRÊN CÙNG */
        .top-hud {
            position: absolute; top: 0; width: 100vw; height: 8vh;
            background: var(--panel-bg); border-bottom: 0.5vh solid var(--border);
            display: flex; align-items: center; justify-content: space-between; padding: 0 2vw; box-sizing: border-box; z-index: 100;
        }

        /* MANA (HÌNH TRÒN ĐỎ) */
        .mana-orb {
            position: absolute; left: 2vw; top: 12vh; width: 12vh; height: 12vh;
            background: radial-gradient(circle, #ff4d4d, #900);
            border: 0.4vh solid #fff; border-radius: 50%; box-shadow: 0 0 2vh var(--mana);
            display: flex; flex-direction: column; align-items: center; justify-content: center; z-index: 50;
        }

        /* KHU VỰC CHIẾN ĐẤU (CAM) */
        .main-battle {
            position: absolute; top: 8vh; width: 100vw; height: 62vh;
            display: flex; justify-content: space-around; align-items: center;
        }
        .unit-box { width: 35vw; text-align: center; position: relative; }
        
        /* HIỂN THỊ HIỆU ỨNG (BUFF ICONS) */
        .buff-list { display: flex; justify-content: center; gap: 0.5vw; margin-bottom: 1vh; height: 4vh; }
        .buff-icon { position: relative; font-size: 2.2vw; cursor: help; transition: 0.2s; }
        .buff-icon:hover { transform: scale(1.2); }
        .buff-dur { position: absolute; bottom: -5px; right: -5px; background: #800; color: white; border-radius: 50%; padding: 1px 5px; font-size: 0.7vw; border: 1px solid white; font-weight: bold; }

        /* Ý ĐỊNH QUÁI VẬT (INTENT) */
        .intent-bubble {
            position: absolute; top: -10vh; left: 50%; transform: translateX(-50%);
            background: rgba(0,0,0,0.85); border: 2px solid var(--border);
            padding: 8px 20px; border-radius: 20px; color: #ffa502; font-weight: bold; font-size: 1.1vw; 
            text-transform: uppercase; box-shadow: 0 0 10px gold;
        }

        .avatar-img { width: 25vw; height: 25vw; object-fit: contain; }
        .hero-glow { filter: drop-shadow(0 0 1.5vh cyan); }
        .monster-glow { filter: drop-shadow(0 0 2vh red); }

        .hp-bar { width: 25vw; height: 3.5vh; background: #222; border: 0.3vh solid #fff; border-radius: 2vh; margin: 1vh auto; position: relative; overflow: hidden; }
        .hp-fill { height: 100%; background: var(--hp); transition: 0.8s cubic-bezier(0.18, 0.89, 0.32, 1.28); }
        .hp-text { position: absolute; width: 100%; text-align: center; line-height: 3.5vh; font-weight: bold; font-size: 1.1vw; top: 0; text-shadow: 1px 1px 2px #000; }

        /* DASHBOARD DƯỚI CÙNG */
        .bottom-dashboard {
            position: absolute; bottom: 0; width: 100vw; height: 30vh;
            display: flex; align-items: flex-end; background: linear-gradient(to top, #000, transparent);
        }

        .stats-zone { width: 15vw; height: 100%; display: flex; align-items: center; justify-content: center; gap: 2vw; }
        .v-bar { width: 2.2vw; height: 22vh; background: rgba(0,0,0,0.8); border: 0.2vh solid #00d2ff; display: flex; flex-direction: column-reverse; }
        .v-seg { width: 100%; height: 9%; margin-top: 1px; transition: 0.3s; }
        .s-on { background: #00d2ff; box-shadow: 0 0 1vh #00d2ff; }
        .e-on { background: #00ff88; box-shadow: 0 0 1vh #00ff88; }

        .hand-zone { flex: 1; height: 28vh; display: flex; justify-content: center; align-items: center; gap: 1vw; }
        .card { width: 10vw; height: 22vh; background: #fff; color: #000; border-radius: 1vh; transition: 0.3s; cursor: pointer; display: flex; flex-direction: column; overflow: hidden; border: 2px solid #555; }
        .card:hover { transform: translateY(-5vh) scale(1.05); border-color: gold; box-shadow: 0 2vh 4vh rgba(255,255,255,0.5); }
        .card-img { width: 100%; height: 55%; object-fit: cover; }

        .btn-end { padding: 2vh 2.5vw; background: linear-gradient(#d35400, #8b0000); color: white; border: 0.4vh solid var(--border); border-radius: 4vh; font-family: 'Cinzel'; cursor: pointer; font-size: 1.2vw; transition: 0.2s; }
        .btn-end:hover { transform: scale(1.1); filter: brightness(1.2); }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="battle-container">
            <div class="top-hud">
                <div style="font-family:'Cinzel'; font-size:1.8vw;"><asp:Label ID="lblPlayerName" runat="server" /></div>
                <div style="color:cyan; font-family:'Cinzel'; font-size:1.4vw;">NĂNG LƯỢNG: <asp:Label ID="lblTopMana" runat="server" /> / 3</div>
                <div style="color:var(--gold); font-weight:bold; border: 2px solid gold; border-radius: 20px; padding: 5px 20px;">💰 <asp:Label ID="lblGold" runat="server" /></div>
            </div>

            <div class="mana-orb">
                <span style="font-size:1.5vh; font-weight:bold;">MANA</span>
                <span style="font-size:5vh; font-weight:bold;"><asp:Label ID="lblMana" runat="server" /></span>
            </div>

            <div class="main-battle">
                <div class="unit-box">
                    <div class="buff-list">
                        <asp:Repeater ID="rptHeroBuffs" runat="server">
                            <ItemTemplate>
                                <div class="buff-icon" title='<%# Eval("Description") %>'>
                                    <%# Eval("Icon") %><span class="buff-dur" runat="server" visible='<%# (int)Eval("Duration") < 900 %>'><%# Eval("Duration") %></span>
                                    <span class="buff-dur" runat="server" visible='<%# (int)Eval("Duration") >= 900 %>'>∞</span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <div style="display:flex; justify-content:center; gap:2vw; font-family:'Cinzel'; font-size:1.1vw; margin-bottom:1vh; color:gold;">
                        <span>🛡️ GIÁP: <asp:Label ID="lblArmor" runat="server" Text="0" /></span>
                        <span>⚔️ CÔNG: <asp:Label ID="lblPlayerStr" runat="server" /></span>
                    </div>
                    <img id="imgHero" runat="server" class="avatar-img hero-glow" src="~/Images/hero.png" />
                    <div class="hp-bar">
                        <div id="pbPlayer" runat="server" class="hp-fill"></div>
                        <div class="hp-text"><asp:Literal ID="litPlayerHP" runat="server" /></div>
                    </div>
                </div>

                <div class="unit-box">
                    <div id="divIntent" runat="server" class="intent-bubble">
                        <asp:Label ID="lblMonsterIntent" runat="server" />
                    </div>
                    <div class="buff-list">
                        <asp:Repeater ID="rptMonsterBuffs" runat="server">
                            <ItemTemplate>
                                <div class="buff-icon" title='<%# Eval("Description") %>'>
                                    <%# Eval("Icon") %><span class="buff-dur" runat="server" visible='<%# (int)Eval("Duration") < 900 %>'><%# Eval("Duration") %></span>
                                    <span class="buff-dur" runat="server" visible='<%# (int)Eval("Duration") >= 900 %>'>∞</span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <img id="imgEnemy" runat="server" class="avatar-img monster-glow" />
                    <div class="hp-bar">
                        <div id="pbEnemy" runat="server" class="hp-fill"></div>
                        <div class="hp-text"><asp:Literal ID="litEnemyHP" runat="server" /></div>
                    </div>
                    <div id="lblEnemyName" runat="server" style="color:#ff4757; font-weight:bold; font-family:'Cinzel'; font-size:1.8vw;"></div>
                </div>
            </div>

            <div class="bottom-dashboard">
                <div class="stats-zone">
                    <div style="text-align:center;">🧠<div class="v-bar"><asp:Literal ID="litStress" runat="server" /></div></div>
                    <div style="text-align:center;">🔋<div class="v-bar"><asp:Literal ID="litExhaust" runat="server" /></div></div>
                </div>

                <div class="hand-zone">
                    <asp:Repeater ID="rptHand" runat="server" OnItemCommand="rptHand_ItemCommand">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnCard" runat="server" CommandName="PlayCard" CommandArgument='<%# Container.ItemIndex %>' CssClass="card" style="text-decoration:none;">
                                <div style="background:#222; color:#fff; padding:0.5vh; font-weight:bold; text-align:center; font-size:0.9vw; border-bottom: 1px solid gold;">
                                    ⚡<%# Eval("HaoTonMana") %> | <%# Eval("TenThe") %>
                                </div>
                                <img src='<%# ResolveUrl("~/Images/" + Eval("TenHinhAnh")) %>' class="card-img" />
                                <div style="padding:0.8vh; font-size:0.75vw; text-align:center; color:#111; font-weight:600;">
                                    <%# GetDynamicDesc(Eval("MaThe"), Eval("CongSatThuong"), Eval("GiaTriGiap"), Eval("MoTa")) %>
                                </div>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <div class="end-turn-zone">
                    <asp:Button ID="btnEndTurn" runat="server" Text="KẾT THÚC LƯỢT" CssClass="btn-end" OnClick="btnEndTurn_Click" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>