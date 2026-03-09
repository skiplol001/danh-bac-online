<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="gamethebaiv2.Game.Play" %>
<!DOCTYPE html>
<html lang="vi">
<head runat="server">
    <meta charset="UTF-8">
    <title>Bản Đồ Hành Trình - Hầm Ngục Tử Chiến</title>
    <link href="https://fonts.googleapis.com/css2?family=Cinzel:wght@700&display=swap" rel="stylesheet">
    <style>
        :root { --hp: #ff4757; --mana: #2e86de; --gold: #eab543; --bg: #050505; --node-size: 5vw; --blue-trail: #3498db; }
        body, html { margin: 0; padding: 0; height: 100%; font-family: 'Segoe UI', sans-serif; background: var(--bg); color: white; overflow: hidden; }
        .main-layout { display: flex; width: 100vw; height: 100vh; }

        /* BẢN ĐỒ CHỐNG LỆCH KHI ZOOM */
        .map-section { flex: 1; position: relative; overflow-y: scroll; background: linear-gradient(rgba(0,0,0,0.85), rgba(0,0,0,0.85)), url('../Images/dungeon_bg.png') center/cover; border-right: 2px solid #333; scrollbar-width: none; }
        .map-content { width: 100%; display: flex; flex-direction: column-reverse; align-items: center; padding: 20vh 0; position: relative; min-height: 400vh; }
        #map-canvas { position: absolute; top: 0; left: 0; width: 100%; height: 100%; pointer-events: none; z-index: 5; }

        .floor-row { display: block; position: relative; width: 75vw; height: 25vh; z-index: 10; margin: 2vh 0; }

        /* THIẾT KẾ NÚT THEO TRẠNG THÁI */
        .node { 
            width: var(--node-size); height: var(--node-size); background: rgba(15,15,15,0.98); 
            border: 0.25vw solid #444; border-radius: 50%; display: flex; align-items: center; justify-content: center; 
            font-size: 2.2vw; position: absolute; text-decoration: none; transition: 0.3s; z-index: 20;
            top: 50%; transform: translate(-50%, -50%); 
        }
        
        /* Nút đã đi qua: Giữ sáng màu xanh */
        .node.visited { border-color: var(--blue-trail) !important; box-shadow: 0 0 1.5vw rgba(52,152,219,0.6); filter: none !important; opacity: 1 !important; }
        /* Nút hiện tại: Xanh lá rực rỡ */
        .node.current { border-color: #00ff88; box-shadow: 0 0 2.5vw #00ff88; transform: translate(-50%, -50%) scale(1.15); z-index: 30; }
        /* Nút có thể đi: Viền vàng nhấp nháy */
        .node.reachable { border-color: var(--gold); cursor: pointer; animation: glow 1.5s infinite; }
        /* Lựa chọn bị bỏ qua: Nhuộm xám và khóa */
        .node.locked { opacity: 0.15; filter: grayscale(100%); pointer-events: none; }

        @keyframes glow { 0%, 100% { box-shadow: 0 0 0.5vw var(--gold); } 50% { box-shadow: 0 0 1.8vw var(--gold); } }

        /* ICONS */
        .node.CombatLow::before { content: "⚔️"; }
        .node.CombatMid::before { content: "🛡️"; }
        .node.CombatHigh::before { content: "☠️"; }
        .node.Shop::before { content: "💰"; color: var(--gold); }
        .node.Event::before { content: "❓"; }
        .node.Boss::before { content: "👹"; font-size: 4vw; }

        /* PANEL SỰ KIỆN */
        .event-overlay { position: fixed; inset: 0; z-index: 2000; background: rgba(0,0,0,0.97); display: flex; align-items: center; justify-content: center; }
        .event-card { width: 45vw; background: #0a0a0a; border: 0.2vw solid var(--gold); padding: 3vw; border-radius: 1vw; text-align: center; font-family: 'Cinzel', serif; box-shadow: 0 0 5vw #000; }
        .btn-choice { display: block; width: 100%; padding: 1.2vw; margin: 1vw 0; background: #1a1a1a; color: white; border: 0.1vw solid #444; cursor: pointer; transition: 0.3s; font-size: 1.1vw; border-radius: 0.4vw; font-family: 'Cinzel'; }
        .btn-choice:hover { background: #7d0000; border-color: var(--gold); }

        /* SIDEBAR TRẠNG THÁI */
        .sidebar-section { width: 25vw; background: #080808; border-left: 2px solid #333; padding: 2vw; display: flex; flex-direction: column; z-index: 100; }
        .stat-bar { width: 100%; height: 1.8vw; background: #111; border-radius: 0.9vw; margin-bottom: 1vw; position: relative; border: 1px solid #333; overflow: hidden; }
        .stat-fill { height: 100%; transition: 0.8s ease-in-out; }
        .stat-text { position: absolute; width: 100%; text-align: center; top: 0; font-size: 0.9vw; font-weight: bold; line-height: 1.8vw; text-shadow: 1px 1px 2px #000; color: white; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
       <asp:Panel ID="pnlEvent" runat="server" Visible="false">
        <h2 id="eventTitle" runat="server"></h2>
        <p id="eventDesc" runat="server"></p>
        <asp:Button ID="btnChoice1" runat="server" OnClick="Choice_Click" CommandArgument="1" />
        <asp:Button ID="btnChoice2" runat="server" OnClick="Choice_Click" CommandArgument="2" />
        <asp:Button ID="btnChoice3" runat="server" OnClick="Choice_Click" CommandArgument="3" />
    </asp:Panel>
    
    <asp:HiddenField ID="HiddenField1" runat="server" />

        <div class="main-layout">
            <div class="map-section" id="mapWrapper">
                <div class="map-content" id="mapContent">
                    <svg id="map-canvas"></svg>
                    <asp:Repeater ID="rptFloors" runat="server">
                        <ItemTemplate>
                            <div class="floor-row">
                                <asp:Repeater ID="rptNodes" runat="server" DataSource='<%# GetNodes(Eval("Tang")) %>'>
                                    <ItemTemplate>
                                        <a href='<%# GetNodeStatus(Eval("NutID")) == "reachable" ? "Play.aspx?node=" + Eval("NutID") : "javascript:void(0);" %>' 
                                           class='<%# "node " + Eval("LoaiNut") + " " + GetNodeStatus(Eval("NutID")) %>'
                                           style='left: calc(50% + <%# (Convert.ToDouble(Eval("ViTriX")) * 22) %>vw);'
                                           data-id='<%# Eval("NutID") %>'>
                                        </a>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>

            <div class="sidebar-section">
                <h2 style="font-family:'Cinzel'; text-align:center; color:var(--gold); font-size:2vw; margin-bottom:1.5vw;">TRẠNG THÁI</h2>
                <div style="height:22vw; border:0.15vw solid var(--gold); margin-bottom:2vw; overflow:hidden; background:#000;">
                    <img src="../Images/hero.png" style="width:100%; height:100%; object-fit:cover; mix-blend-mode:lighten;" />
                </div>
                <div class="stat-bar"><div id="pbHP" runat="server" class="stat-fill" style="background:var(--hp);"></div><div class="stat-text">HP: <asp:Label ID="lblHP" runat="server" /></div></div>
                <div class="stat-bar"><div id="pbStress" runat="server" class="stat-fill" style="background:#a29bfe;"></div><div class="stat-text">STRESS: <asp:Label ID="lblStress" runat="server" /></div></div>
                <div class="stat-bar"><div id="pbExhaust" runat="server" class="stat-fill" style="background:#ff9f43;"></div><div class="stat-text">KIỆT SỨC: <asp:Label ID="lblExhaust" runat="server" /></div></div>
                <div style="display:flex; justify-content:space-between; margin-top:1.5vw; font-family:'Cinzel'; font-size:1.6vw;">
                    <span style="color:var(--gold)">💰 <asp:Label ID="lblGold" runat="server" /></span>
                    <span style="color:#ff7675">⚔️ <asp:Label ID="lblATK" runat="server" /></span>
                </div>
                <a href="Deck.aspx" style="margin-top:auto; text-decoration:none; text-align:center; padding:1.5vw; background:#440000; color:white; border:0.2vw solid var(--gold); font-family:'Cinzel'; font-weight:bold; font-size:1.3vw; transition:0.3s;">HÀNH TRANG THẺ BÀI</a>
            </div>
        </div>

        <asp:Repeater ID="rptPaths" runat="server">
            <ItemTemplate><span class="path-data" data-from='<%# Eval("NutTruocID") %>' data-to='<%# Eval("NutSauID") %>'></span></ItemTemplate>
        </asp:Repeater>
        <asp:HiddenField ID="hfHistory" runat="server" /> </form>

    <script>
        function drawPaths() {
            const svg = document.getElementById('map-canvas');
            const content = document.getElementById('mapContent');
            const historyStr = document.getElementById('<%= hfHistory.ClientID %>').value;
            const history = historyStr.split(',').filter(x => x); // Mảng ID đã đi
            const curId = document.querySelector('.node.current')?.getAttribute('data-id');

            svg.setAttribute('height', content.scrollHeight);
            svg.setAttribute('width', content.clientWidth);
            svg.innerHTML = '';
            const contentRect = content.getBoundingClientRect();

            document.querySelectorAll('.path-data').forEach(p => {
                const fId = p.dataset.from;
                const tId = p.dataset.to;
                const fEl = document.querySelector(`[data-id="${fId}"]`);
                const tEl = document.querySelector(`[data-id="${tId}"]`);

                if (fEl && tEl) {
                    const fRect = fEl.getBoundingClientRect();
                    const tRect = tEl.getBoundingClientRect();
                    const x1 = (fRect.left + fRect.width / 2) - contentRect.left;
                    const y1 = (fRect.top + fRect.height / 2) - contentRect.top + content.scrollTop;
                    const x2 = (tRect.left + tRect.width / 2) - contentRect.left;
                    const y2 = (tRect.top + tRect.height / 2) - contentRect.top + content.scrollTop;

                    const line = document.createElementNS('http://www.w3.org/2000/svg', 'path');
                    line.setAttribute('d', `M ${x1} ${y1} C ${x1} ${(y1 + y2) / 2}, ${x2} ${(y1 + y2) / 2}, ${x2} ${y2}`);
                    line.setAttribute('fill', 'none');

                    // --- LOGIC MÀU SẮC THEO YÊU CẦU LƯU VẾT ---
                    const isFromHistory = history.includes(fId);
                    const isToHistory = history.includes(tId) || tId === curId;

                    if (isFromHistory && isToHistory) {
                        // 1. XANH DƯƠNG: Giữ lại toàn bộ hành trình đã qua
                        line.setAttribute('stroke', '#3498db');
                        line.setAttribute('stroke-width', '0.7vw');
                        line.setAttribute('filter', 'drop-shadow(0 0 1vw rgba(52,152,219,0.8))');
                    }
                    else if (fId === curId && tEl.classList.contains('reachable')) {
                        // 2. ĐỎ NÉT ĐỨT: Các hướng đi có thể rẽ nhánh từ vị trí hiện tại
                        line.setAttribute('stroke', '#ff4757');
                        line.setAttribute('stroke-width', '0.35vw');
                        line.setAttribute('stroke-dasharray', '1vw, 0.5vw');
                    }
                    else {
                        // 3. KHÁC: Làm mờ các đường bị khóa hoặc bỏ qua
                        line.setAttribute('stroke', '#222');
                        line.setAttribute('stroke-width', '0.1vw');
                        line.setAttribute('opacity', '0.2');
                    }
                    svg.appendChild(line);
                }
            });
        }
        window.onload = drawPaths;
        window.onresize = drawPaths;
        window.addEventListener('wheel', (e) => { if (e.ctrlKey) setTimeout(drawPaths, 150); });
    </script>
</body>
</html>