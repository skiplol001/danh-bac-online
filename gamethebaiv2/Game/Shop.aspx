<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Shop.aspx.cs" Inherits="gamethebaiv2.Game.Shop" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Cửa Hàng Cổ Thuật</title>
    <link href="https://fonts.googleapis.com/css2?family=Cinzel:wght@700&display=swap" rel="stylesheet">
    <style>
        :root { --gold: #eab543; --bg: #0a0a0a; --card-bg: #f4ecd8; }
        body { background: radial-gradient(circle, #1a1a1a, #050505); color: white; font-family: 'Segoe UI', sans-serif; margin: 0; }
        .shop-container { max-width: 1000px; margin: 50px auto; text-align: center; }
        
        .gold-display { font-family: 'Cinzel'; color: var(--gold); font-size: 28px; margin-bottom: 30px; text-shadow: 0 0 10px rgba(234, 181, 67, 0.5); }
        
        /* KHU VỰC BÁN BÀI */
        .shelf { display: flex; justify-content: center; gap: 30px; margin-bottom: 50px; }
        .shop-item { background: rgba(255,255,255,0.05); padding: 20px; border: 1px solid #333; border-radius: 10px; transition: 0.3s; }
        .shop-item:hover { border-color: var(--gold); background: rgba(255,255,255,0.1); }
        
        .card-preview { width: 140px; height: 200px; background: var(--card-bg); border-radius: 8px; color: black; margin-bottom: 15px; position: relative; }
        .price-tag { color: var(--gold); font-weight: bold; margin-bottom: 10px; display: block; }
        .btn-buy { background: #27ae60; color: white; border: none; padding: 8px 20px; cursor: pointer; border-radius: 5px; font-family: 'Cinzel'; }
        .btn-buy:disabled { background: #555; cursor: not-allowed; }

        /* KHU VỰC DỊCH VỤ */
        .service-box { background: rgba(139, 0, 0, 0.2); border: 2px solid #8b0000; padding: 30px; border-radius: 15px; display: inline-block; }
        .btn-exit { display: block; margin: 40px auto; padding: 15px 50px; background: transparent; color: white; border: 2px solid #555; cursor: pointer; text-decoration: none; width: fit-content; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="shop-container">
            <h1 style="font-family:'Cinzel'; font-size: 45px; letter-spacing: 5px;">CỬA HÀNG CỔ THUẬT</h1>
            
            <div class="gold-display"> túi tiền: 💰 <asp:Label ID="lblGold" runat="server" /></div>

            <div class="shelf">
                <asp:Repeater ID="rptCards" runat="server" OnItemCommand="rptCards_ItemCommand">
                    <ItemTemplate>
                        <div class="shop-item">
                            <div class="card-preview">
                                <div style="padding: 10px; font-size: 12px; font-weight: bold;"><%# Eval("TenThe") %></div>
                                <img src='<%# "../Images/" + Eval("TenHinhAnh") %>' style="width: 80%; height: 80px; object-fit: cover;" />
                                <div style="font-size: 10px; padding: 5px;"><%# Eval("MoTa") %></div>
                            </div>
                            <span class="price-tag">75 VÀNG</span>
                            <asp:Button runat="server" Text="MUA" CssClass="btn-buy" 
                                CommandName="BuyCard" CommandArgument='<%# Eval("TheID") %>' />
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <div class="service-box">
                <h3 style="margin-top: 0; color: #ff4757;">DỊCH VỤ Y TẾ</h3>
                <p>Hồi phục 25 Máu ngay lập tức</p>
                <span class="price-tag">50 VÀNG</span>
                <asp:Button ID="btnHeal" runat="server" Text="HỒI MÁU" CssClass="btn-buy" 
                    OnClick="btnHeal_Click" style="background:#c0392b" />
            </div>

            <a href="Play.aspx" class="btn-exit">QUAY LẠI BẢN ĐỒ</a>
        </div>
    </form>
</body>
</html>