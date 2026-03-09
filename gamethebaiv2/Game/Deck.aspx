<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Deck.aspx.cs" Inherits="gamethebaiv2.Game.Deck" %>

<!DOCTYPE html>
<html lang="vi">
<head runat="server">
    <meta charset="UTF-8">
    <title>Hành Trang Thẻ Bài - Huyền Thoại</title>
    <link href="https://fonts.googleapis.com/css2?family=Cinzel:wght@700&display=swap" rel="stylesheet">
    <style>
        :root { --gold: #d4af37; --bg: #0a0a0a; --mana: #2e86de; --card-bg: #f4ecd8; }
        body { 
            background: radial-gradient(circle, #1a1a1a, #050505); 
            color: white; font-family: 'Segoe UI', sans-serif; margin: 0; min-height: 100vh;
        }

        .container { max-width: 1200px; margin: 0 auto; padding: 40px 20px; }
        
        .header { text-align: center; margin-bottom: 50px; }
        .header h1 { font-family: 'Cinzel', serif; color: var(--gold); font-size: 42px; text-transform: uppercase; letter-spacing: 5px; margin: 0; }
        .header p { color: #888; font-style: italic; }

        /* LƯỚI THẺ BÀI */
        .card-grid { 
            display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); 
            gap: 25px; justify-items: center; 
        }

        /* THIẾT KẾ THẺ BÀI (GIỐNG TRONG TRẬN ĐÁNH) */
        .card-item { 
            width: 160px; height: 230px; background: var(--card-bg); border: 3px solid #333; 
            border-radius: 10px; color: #000; position: relative; transition: 0.4s; 
            display: flex; flex-direction: column; cursor: default; overflow: hidden;
            box-shadow: 0 10px 20px rgba(0,0,0,0.5);
        }
        .card-item:hover { transform: translateY(-15px) rotate(2deg); border-color: var(--gold); box-shadow: 0 15px 30px rgba(212,175,55,0.3); }

        .card-mana { 
            position: absolute; top: -10px; left: -10px; width: 35px; height: 35px; 
            background: var(--mana); color: white; border-radius: 50%; 
            display: flex; align-items: center; justify-content: center; 
            font-weight: bold; border: 2px solid white; z-index: 10;
        }

        .card-name { font-family: 'Cinzel', serif; font-size: 12px; font-weight: bold; text-align: center; padding: 8px 4px; border-bottom: 1px solid #ccc; background: rgba(0,0,0,0.05); }
        .card-img-box { height: 100px; margin: 5px; border: 1px solid #ddd; background: #eee; overflow: hidden; }
        .card-img-box img { width: 100%; height: 100%; object-fit: cover; }
        .card-desc { flex: 1; padding: 8px; font-size: 11px; text-align: center; font-style: italic; line-height: 1.4; color: #333; }
        .card-footer { font-size: 9px; text-align: right; padding: 4px 8px; background: #ddd; font-weight: bold; text-transform: uppercase; }

        /* NÚT QUAY LẠI */
        .back-btn { 
            display: inline-block; margin-top: 50px; padding: 15px 40px; 
            background: transparent; color: var(--gold); border: 2px solid var(--gold); 
            text-decoration: none; font-family: 'Cinzel', serif; transition: 0.3s;
            border-radius: 5px;
        }
        .back-btn:hover { background: var(--gold); color: black; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="header">
                <h1>Hành Trang Thẻ Bài</h1>
                <p>Những cổ thuật và kỹ năng bạn đã thu thập được...</p>
            </div>

            <div class="card-grid">
                <asp:Repeater ID="rptMyDeck" runat="server">
                    <ItemTemplate>
                        <div class="card-item">
                            <div class="card-mana"><%# Eval("HaoTonMana") %></div>
                            <div class="card-name"><%# Eval("TenThe") %></div>
                            <div class="card-img-box">
                                <img src='<%# ResolveUrl("~/Images/" + Eval("TenHinhAnh")) %>' alt="card image" />
                            </div>
                            <div class="card-desc">
                                <%# Eval("MoTa") %>
                            </div>
                            <div class="card-footer">
                                <%# Eval("LoaiThe") %>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            
            <div style="text-align:center;">
<a href='<%= ResolveUrl("~/Default.aspx") %>' class="back-btn">Quay lại Bản đồ</a>

            </div>
        </div>
    </form>
</body>
</html>