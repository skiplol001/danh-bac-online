using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace gamethebaiv2.Game
{
    public partial class Deck : System.Web.UI.Page
    {
        // Lấy chuỗi kết nối từ Web.config
        string strConn = ConfigurationManager.ConnectionStrings["GameTheBai"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Kiểm tra nếu chưa có RunID (người chơi vào thẳng link) thì đẩy về map
            if (Session["CurrentRunID"] == null)
            {
                Response.Redirect("Play.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadCurrentDeck();
            }
        }

        private void LoadCurrentDeck()
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    conn.Open();
                    // Câu lệnh SQL lấy thông tin thẻ bài dựa trên RunID hiện tại
                    string sql = @"
                        SELECT t.TenThe, t.HaoTonMana, t.TenHinhAnh, t.MoTa, t.LoaiThe 
                        FROM HanhTrangThe h
                        INNER JOIN ThuVienTheBai t ON h.TheID = t.TheID
                        WHERE h.RunID = @rid
                        ORDER BY t.HaoTonMana ASC, t.LoaiThe DESC";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@rid", Session["CurrentRunID"]);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Đổ dữ liệu vào Repeater
                    rptMyDeck.DataSource = dt;
                    rptMyDeck.DataBind();
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi nếu cần thiết
                    Response.Write("<script>alert('Lỗi tải hành trang: " + ex.Message + "');</script>");
                }
            }
        }
    }
}