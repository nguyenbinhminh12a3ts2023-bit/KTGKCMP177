import docx
from docx import Document
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml import OxmlElement, parse_xml
from docx.oxml.ns import nsdecls, qn

def create_report():
    doc = Document()

    # Set page margins
    for section in doc.sections:
        section.top_margin = Inches(1)
        section.bottom_margin = Inches(1)
        section.left_margin = Inches(1)
        section.right_margin = Inches(1)

    # Styles
    teal = RGBColor(8, 127, 130)       # #087F82
    navy = RGBColor(24, 50, 79)        # #18324F
    charcoal = RGBColor(30, 41, 59)

    # Title
    p_title = doc.add_paragraph()
    p_title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r_sub = p_title.add_run("BÁO CÁO BÀI TẬP GIỮA KỲ\n")
    r_sub.font.size = Pt(14)
    r_sub.font.bold = True
    r_sub.font.color.rgb = navy

    r_main = p_title.add_run("HỆ THỐNG QUẢN LÝ VÀ THI THỬ TOEIC READING\n")
    r_main.font.size = Pt(18)
    r_main.font.bold = True
    r_main.font.color.rgb = teal

    r_tech = p_title.add_run("ASP.NET Core MVC (ABP v10.2.0) & Flutter Dart Mobile App")
    r_tech.font.size = Pt(12)
    r_tech.font.italic = True
    r_tech.font.color.rgb = RGBColor(100, 116, 139)

    doc.add_paragraph()

    # Metadata box
    t_meta = doc.add_table(rows=4, cols=2)
    t_meta.alignment = WD_TABLE_ALIGNMENT.CENTER
    meta_data = [
        ("Đề tài:", "Xây dựng hệ thống quản lý đề thi Word (.docx) và ứng dụng thi thử TOEIC Reading"),
        ("Kiến trúc Backend:", "ASP.NET Core MVC, ABP Framework v10.2.0, Entity Framework Core, SQL Server"),
        ("Kiến trúc Frontend:", "Flutter Mobile & Web Application (Dart), Provider Pattern, Material Design 3"),
        ("Dữ liệu nguồn:", "ETS TOEIC 2019 Test 07 Reading (100 câu: Part 5, Part 6, Part 7)"),
    ]
    for row_idx, (k, v) in enumerate(meta_data):
        row = t_meta.rows[row_idx]
        cell_0 = row.cells[0]
        cell_1 = row.cells[1]
        cell_0.paragraphs[0].add_run(k).bold = True
        cell_1.paragraphs[0].add_run(v)
        cell_0.width = Inches(2.0)
        cell_1.width = Inches(4.5)

    doc.add_paragraph()

    # Chapter 1
    h1 = doc.add_heading("1. TỔNG QUAN YÊU CẦU ĐỀ TÀI", level=1)
    h1.runs[0].font.color.rgb = teal

    doc.add_paragraph(
        "Theo yêu cầu của đề bài 'Đề thi giữa kỳ', hệ thống gồm hai phân hệ chính:\n"
        "1. Quản trị viên (Admin Web): Phát triển trên nền tảng mã nguồn có sẵn tại thư mục 10.2.0 (ASP.NET Boilerplate v10.2.0, EF Core, Microsoft SQL Server). "
        "Cho phép Admin import đề thi từ tệp Word (.docx) được gán các thẻ định dạng chuẩn; kiểm tra tính hợp lệ nghiêm ngặt (chấp nhận khi 100% đúng, từ chối toàn bộ nếu có bất kỳ lỗi nào); "
        "xem trước và chỉnh sửa câu hỏi trực tiếp trên giao diện Web.\n"
        "2. Thí sinh (Mobile App): Xây dựng ứng dụng di động hoàn chỉnh bằng Flutter & Dart, bám sát 100% thiết kế giao diện Figma đã cung cấp, "
        "đồng bộ dữ liệu thông qua hệ thống RESTful API của Backend."
    )

    # Chapter 2
    h2 = doc.add_heading("2. QUY CHUẨN THẺ TAG TRONG FILE WORD (.DOCX)", level=1)
    h2.runs[0].font.color.rgb = teal

    doc.add_paragraph(
        "File Word 'DeThiGiuaKy_Reading_v07.docx' được trích xuất hoàn chỉnh từ 'Reading v07.pdf' (100 câu ETS 2019 Test 7) với quy ước tags chuẩn xác:"
    )

    t_tag = doc.add_table(rows=9, cols=3)
    t_tag.alignment = WD_TABLE_ALIGNMENT.CENTER
    tag_headers = ["Thẻ Định Dạng", "Ý Nghĩa", "Ví Dụ Sử Dụng"]
    for i, h in enumerate(tag_headers):
        cell = t_tag.rows[0].cells[i]
        p = cell.paragraphs[0]
        r = p.add_run(h)
        r.bold = True
        r.font.color.rgb = RGBColor(255, 255, 255)
        # Shading
        shading = parse_xml(r'<w:shd {} w:fill="087F82"/>'.format(nsdecls('w')))
        cell._tc.get_or_add_tcPr().append(shading)

    tag_rows = [
        ("[TEST]", "Bắt đầu bài thi / đề thi", "[TEST] ETS 2019 Reading Test 07"),
        ("[PART: X]", "Khai báo phần thi (Part 5, 6, 7)", "[PART: 5] Incomplete Sentences"),
        ("[GROUP: X-Y]", "Nhóm câu hỏi chung đoạn văn", "[GROUP: 131-134] Text Completion"),
        ("[PASSAGE]", "Nội dung đoạn văn / bài đọc", "[PASSAGE] To: All Staff..."),
        ("[QUESTION: N]", "Nội dung câu hỏi số N", "[QUESTION: 101] Ms. Hughes --- to attend..."),
        ("[BLANK: N]", "Vị trí điền khuyết (Part 6)", "[BLANK: 131]"),
        ("[A] [B] [C] [D]", "Nội dung 4 đáp án lựa chọn", "[A] plan  [B] planned  [C] planning  [D] plans"),
        ("[ANSWER: N] X", "Đáp án chính thức của câu hỏi", "[ANSWER: 101] B"),
    ]
    for row_idx, (t, m, e) in enumerate(tag_rows, start=1):
        row = t_tag.rows[row_idx]
        row.cells[0].paragraphs[0].add_run(t).bold = True
        row.cells[1].paragraphs[0].add_run(m)
        row.cells[2].paragraphs[0].add_run(e)

    doc.add_paragraph()

    # Chapter 3
    h3 = doc.add_heading("3. KIẾN TRÚC VÀ CƠ SỞ DỮ LIỆU BACKEND (ASP.NET CORE)", level=1)
    h3.runs[0].font.color.rgb = teal

    doc.add_paragraph(
        "Hệ thống Backend được tổ chức chuẩn theo mô hình đa tầng của ABP Framework (DDD - Domain Driven Design):\n"
        "• Core Layer: Định nghĩa các thực thể dữ liệu gồm ToeicExam, ToeicPart, ToeicPassageGroup, ToeicQuestion, ToeicAnswerOption, ExamSubmission, ExamSubmissionDetail.\n"
        "• EntityFrameworkCore Layer: comDbContext quản lý các DbSet, tự động migration và seeding dữ liệu từ file JSON trích xuất.\n"
        "• Application Layer: Dịch vụ ToeicExamAppService xử lý nghiệp vụ giải mã XML từ file Word (.docx) native không phụ thuộc thư viện ngoài (dùng ZipArchive + XDocument), kiểm tra tính hợp lệ và tự động chấm điểm theo thang điểm chuẩn TOEIC (0 - 495 điểm).\n"
        "• Web.Mvc Layer: Cung cấp Web UI cho Quản trị viên (Views/ToeicAdmin) và bộ RESTful APIs cho ứng dụng di động."
    )

    # Chapter 4
    h4 = doc.add_heading("4. CÁC MÀN HÌNH THEO THIẾT KẾ FIGMA", level=1)
    h4.runs[0].font.color.rgb = teal

    doc.add_paragraph("Hệ thống đã hiện thực hóa 100% các màn hình trong thiết kế Figma:")

    t_figma = doc.add_table(rows=17, cols=3)
    t_figma.alignment = WD_TABLE_ALIGNMENT.CENTER
    for i, h in enumerate(["Phân Hệ", "Màn Hình (Figma ID)", "Chức Năng & Điểm Nổi Bật"]):
        cell = t_figma.rows[0].cells[i]
        p = cell.paragraphs[0]
        r = p.add_run(h)
        r.bold = True
        r.font.color.rgb = RGBColor(255, 255, 255)
        shading = parse_xml(r'<w:shd {} w:fill="18324F"/>'.format(nsdecls('w')))
        cell._tc.get_or_add_tcPr().append(shading)

    screens = [
        ("Admin Web", "Quản lý đề thi (#4:9787)", "Danh sách đề thi, trạng thái hoạt động, nút Import, Preview, Edit"),
        ("Admin Web", "Nhập đề từ Word (#4:9943)", "Kéo thả/chọn file .docx, hướng dẫn cấu trúc tags chuẩn"),
        ("Admin Web", "Lỗi định dạng (#4:10071)", "Báo cáo lỗi chi tiết: thiếu tag, sai cú pháp, câu/đoạn văn sai; từ chối đề thi"),
        ("Admin Web", "Kiểm tra thành công (#4:10191)", "Báo cáo 100/100 câu hợp lệ, thông tin tổng quan, nút Xác nhận lưu"),
        ("Admin Web", "Xem trước đề thi (#4:10337)", "Hiển thị đầy đủ bài đọc, câu hỏi và đáp án đúng gắn nhãn chuẩn"),
        ("Admin Web", "Chỉnh sửa đề thi (#4:10505)", "Sidebar chọn câu 101-200, form sửa câu hỏi, đáp án, lời giải thích"),
        ("Mobile App", "Đề thi · TOEIC Reading (#4:5713)", "Danh sách đề thi có sẵn, thời gian làm bài, số lượng câu hỏi"),
        ("Mobile App", "Chi tiết · Reading Test 01 (#4:5786)", "Thông tin cấu trúc 3 phần (Part 5, 6, 7), nhập họ tên thí sinh"),
        ("Mobile App", "Làm bài · Part 5 (#4:5844)", "Giao diện làm câu hỏi ngắn ngữ pháp & từ vựng (101 - 130)"),
        ("Mobile App", "Làm bài · Part 6 (#4:5907)", "Hiển thị đoạn văn hoàn chỉnh kèm ô trống [131], [132]..."),
        ("Mobile App", "Làm bài · Part 7 (#4:5978)", "Đoạn văn đơn, đoạn kép, đoạn ba với câu hỏi đọc hiểu (147 - 200)"),
        ("Mobile App", "Tổng quan câu hỏi (#4:6071)", "Lưới 100 ô tròn: Đã làm (xanh), Đang xem (vàng), Chưa làm (xám)"),
        ("Mobile App", "Xác nhận nộp bài (#4:6426)", "Modal thông báo số câu đã làm, số câu chưa làm trước khi nộp"),
        ("Mobile App", "Cảnh báo còn 5 phút (#4:6504)", "Modal tự động cảnh báo khi đồng hồ đếm ngược chạm mốc 05:00"),
        ("Mobile App", "Hết giờ làm bài (#4:6572)", "Modal thông báo hết 75 phút và tự động kích hoạt nộp bài thi"),
        ("Mobile App", "Kết quả bài thi (#4:6634)", "Bảng điểm TOEIC (0-495), thống kê Đúng/Sai/Bỏ qua, chi tiết 100 câu"),
    ]
    for row_idx, (sub, scr, desc) in enumerate(screens, start=1):
        row = t_figma.rows[row_idx]
        row.cells[0].paragraphs[0].add_run(sub).bold = True
        row.cells[1].paragraphs[0].add_run(scr)
        row.cells[2].paragraphs[0].add_run(desc)

    doc.add_paragraph()

    # Chapter 5
    h5 = doc.add_heading("5. KẾT QUẢ KIỂM THỬ VÀ HƯỚNG DẪN CHẠY", level=1)
    h5.runs[0].font.color.rgb = teal

    doc.add_paragraph(
        "1. Kiểm thử Backend ASP.NET Core:\n"
        "• Máy chủ đang chạy tại: https://localhost:44312\n"
        "• Quản lý đề thi Admin: https://localhost:44312/ToeicAdmin\n"
        "• Nhập đề thi từ Word: https://localhost:44312/ToeicAdmin/Import\n"
        "• Xem trước đề thi: https://localhost:44312/ToeicAdmin/Preview/1\n"
        "• Chỉnh sửa đề thi: https://localhost:44312/ToeicAdmin/Edit/1\n"
        "• Tệp lỗi mẫu 'DeThi_LoiDinhDang_Mau.docx' kích hoạt đúng 100% màn hình 'Lỗi định dạng' (Figma #4:10071) và từ chối toàn bộ.\n"
        "• Tệp chuẩn 'DeThiGiuaKy_Reading_v07.docx' kích hoạt 100% màn hình 'Kiểm tra thành công' (Figma #4:10191) và lưu thành công 100 câu.\n\n"
        "2. Kiểm thử Mobile App Flutter:\n"
        "• Thư mục mã nguồn: d:\\BaiTapGiuaKi\\frontend\n"
        "• Lệnh chạy ứng dụng:\n"
        "    cd d:\\BaiTapGiuaKi\\frontend\n"
        "    flutter run -d chrome  (hoặc flutter run -d windows / android)\n"
        "• Mã nguồn đã được kiểm tra bằng lệnh 'flutter analyze' không còn bất kỳ lỗi compile nào."
    )

    doc.save("BaoCao_GiuaKy_Toeic.docx")
    print("Report BaoCao_GiuaKy_Toeic.docx created successfully!")

if __name__ == "__main__":
    create_report()
