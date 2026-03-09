<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="gamethebaiv2.Default" %>

<!DOCTYPE html>
<html lang="vi">
<head runat="server">
    <meta charset="UTF-8">
    <title>Huyền Thoại Viễn Chinh - Menu Chính</title>
    <link href="https://fonts.googleapis.com/css2?family=Noto+Serif+JP:wght@700&family=Playfair+Display:ital,wght@1,600&display=swap" rel="stylesheet">
    <style>
        :root {
            --primary-red: #8b0000; --parchment: #f4ecd8;
            --ink-black: #1a1a1a; --gold-border: #d4af37; --glow: #00ff88;
        }
        body, html { margin: 0; padding: 0; height: 100%; overflow: hidden; font-family: 'Noto Serif JP', serif; background-color: var(--ink-black); }
        
        .main-menu-container {
            height: 100vh; display: flex; flex-direction: column; justify-content: center; align-items: center;
            background: linear-gradient(rgba(0,0,0,0.6), rgba(0,0,0,0.4)), url('https://images.unsplash.com/photo-1493976040374-85c8e12f0c0e?q=80&w=1920&auto=format&fit=crop'); 
            background-size: cover; background-position: center; border: 15px solid var(--ink-black); box-sizing: border-box; position: relative;
        }

        .corner { position: absolute; width: 100px; height: 100px; border: 4px solid var(--gold-border); pointer-events: none; }
        .top-left { top: 20px; left: 20px; border-right: none; border-bottom: none; }
        .bottom-right { bottom: 20px; right: 20px; border-left: none; border-top: none; }

        .title-container { text-align: center; margin-bottom: 50px; z-index: 10; animation: fadeInDown 1.5s ease-out; }
        .game-title {
            font-size: 90px; color: var(--parchment);
            text-shadow: 5px 5px 0px var(--primary-red), 0 0 30px rgba(0,0,0,0.9);
            margin: 0; letter-spacing: 15px; text-transform: uppercase;
        }
        .welcome-text { color: var(--gold-border); font-family: 'Playfair Display', serif; font-size: 24px; margin-top: 15px; }

        .menu-options { display: flex; flex-direction: column; gap: 15px; z-index: 10; animation: fadeInUp 1.5s ease-out; }
        .menu-btn {
            background: rgba(10, 10, 10, 0.9); color: var(--parchment);
            border: 2px solid var(--gold-border); padding: 18px 60px;
            font-size: 22px; font-family: 'Noto Serif JP', serif; cursor: pointer;
            transition: all 0.3s; min-width: 350px; text-align: center; text-decoration: none;
        }
        .menu-btn:hover {
            background: var(--primary-red); color: white; border-color: white;
            box-shadow: 0 0 20px var(--primary-red); transform: scale(1.05);
        }
        /* Hiệu ứng đặc biệt cho nút Tiếp tục */
        .btn-continue { border-color: var(--glow); color: var(--glow); box-shadow: inset 0 0 10px rgba(0,255,136,0.2); }
        .btn-continue:hover { background: #006335; box-shadow: 0 0 25px var(--glow); }

        @keyframes fadeInDown { from { opacity: 0; transform: translateY(-50px); } to { opacity: 1; transform: translateY(0); } }
        @keyframes fadeInUp { from { opacity: 0; transform: translateY(50px); } to { opacity: 1; transform: translateY(0); } }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="main-menu-container">
            <div class="corner top-left"></div>
            <div class="corner bottom-right"></div>

            <div class="title-container">
                <h1 class="game-title">HUYỀN THOẠI</h1>
                <div class="welcome-text">
                    Chào mừng, <asp:Label ID="lblPlayerName" runat="server" Text="Kẻ Lữ Hành" />
                </div>
            </div>

            <div class="menu-options">
                <asp:Button ID="btnContinue" runat="server" Text="TIẾP TỤC HÀNH TRÌNH" 
                    CssClass="menu-btn btn-continue" OnClick="btnContinue_Click" Visible="false" />

                <asp:Button ID="btnNewGame" runat="server" Text="KHỞI ĐẦU MỚI" 
                    CssClass="menu-btn" OnClick="btnNewGame_Click" 
                    OnClientClick="return confirm('Bắt đầu chơi mới sẽ xóa toàn bộ tiến trình hiện tại. Bạn chắc chứ?');" />

                <a href="Game/Deck.aspx" class="menu-btn">HÀNH TRANG THẺ BÀI</a>
                <asp:Button ID="btnExit" runat="server" Text="THOÁT KIẾP" CssClass="menu-btn" OnClick="btnExit_Click" />
            </div>
        </div>
    </form>
</body>
</html>