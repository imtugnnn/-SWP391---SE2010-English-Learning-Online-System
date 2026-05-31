GIT WORKING RULES - SWP PROJECT

KHÔNG COMMIT CODE LỖI
Không commit code đang bị lỗi compile/runtime
Phải test chức năng trước khi commit
Không commit file rác hoặc file không cần thiết
KHÔNG MERGE TRỰC TIẾP VÀO MAIN
Không push hoặc merge trực tiếp vào branch main
Tất cả chức năng phải merge qua Pull Request
Chỉ merge vào main khi code đã ổn định
PULL TRƯỚC KHI PUSH
Luôn pull source mới nhất trước khi code hoặc push
Tránh conflict và mất code của thành viên khác
Lệnh: git pull origin develop

MỖI CHỨC NĂNG DÙNG BRANCH RIÊNG
Mỗi feature hoặc bugfix phải tạo branch riêng
Không làm nhiều chức năng trên cùng một branch
Ví dụ: feature/login feature/task-management bugfix/login-error

COMMIT MESSAGE PHẢI RÕ RÀNG
Commit ngắn gọn, đúng nội dung thay đổi
Một commit chỉ nên thực hiện một mục đích
Ví dụ: feat: add login api fix: resolve dashboard bug docs: update SRS document

Không dùng: update fix bug aaaa

PUSH CODE MỖI NGÀY
Thành viên phải push code thường xuyên
Tránh mất source khi máy lỗi
Giúp team theo dõi tiến độ dễ hơn
BACKUP TRƯỚC KHI DEMO
Backup source code trước demo
Backup database (.sql)
Tạo tag version nếu cần
Ví dụ: git tag sprint-1 git push origin sprint-1

REVIEW TRƯỚC KHI MERGE
Code cần được kiểm tra trước khi merge
Kiểm tra:
Chạy được
Không lỗi
Không conflict
Đúng coding convention
KHÔNG TỰ Ý SỬA CODE NGƯỜI KHÁC
Không chỉnh sửa code của thành viên khác nếu chưa trao đổi
Nếu cần sửa phải thông báo rõ ràng
RESOLVE CONFLICT CẨN THẬN
Đọc kỹ phần conflict trước khi sửa
Không xóa nhầm code
Sau khi resolve phải test lại toàn bộ chức năng liên quan
Ký hiệu conflict: <<<<<<< HEAD Code của mình
Code của người khác

branch-name

GIT FLOW ĐỀ XUẤT

main └── develop ├── feature/login ├── feature/task ├── feature/report └── bugfix/...

QUY TRÌNH LÀM VIỆC

Bước 1: git checkout develop git pull origin develop

Bước 2: git checkout -b feature/function-name

Bước 3: git add . git commit -m "feat: add function"

Bước 4: git push origin feature/function-name

Bước 5: Tạo Pull Request để merge vào develop