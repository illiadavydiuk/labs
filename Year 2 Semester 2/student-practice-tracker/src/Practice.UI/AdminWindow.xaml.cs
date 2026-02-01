using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Implementations;
using Practice.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Practice.Windows
{
    public class StudentImportModel { public string FirstName { get; set; } public string LastName { get; set; } public string Email { get; set; } public string RecordBook { get; set; } }
    public class SimpleItemViewModel { public int Id { get; set; } public string Name { get; set; } public string Type { get; set; } }

    public partial class AdminWindow : Window
    {
        private readonly User _user;
        private readonly AppDbContext _context;
        private readonly AdminService _adminService;
        private readonly PracticeService _practiceService;
        private List<StudentImportModel> _importList;
        private int _editingId;
        private object _editingItem;

        public AdminWindow(User user)
        {
            InitializeComponent();
            _user = user;
            TxtAdminName.Text = $"{_user.LastName} {_user.FirstName}";
            TxtInitials.Text = (!string.IsNullOrEmpty(_user.FirstName) ? _user.FirstName[0].ToString() : "A");

            _context = new AppDbContext();

            var auditRepo = new AuditLogRepository(_context);
            var auditService = new AuditService(auditRepo);
            _adminService = new AdminService(_context, auditService);

            var topicRepo = new InternshipTopicRepository(_context);
            var assignRepo = new InternshipAssignmentRepository(_context);
            var statusRepo = new AssignmentStatusRepository(_context);
            var orgRepo = new OrganizationRepository(_context);
            _practiceService = new PracticeService(topicRepo, assignRepo, statusRepo, orgRepo, auditService);

            LoadAllData();
        }

        private async void LoadAllData()
        {
            var groups = await _adminService.GetAllGroupsAsync();
            CmbStudGroup.ItemsSource = CmbImportGroup.ItemsSource = CmbFilterGroup.ItemsSource = EditStudGroup.ItemsSource = groups;

            var depts = await _adminService.GetAllDepartmentsAsync();
            CmbSupDept.ItemsSource = CmbFilterDept.ItemsSource = EditSupDept.ItemsSource = depts;

            var positions = await _adminService.GetAllPositionsAsync();
            CmbSupPos.ItemsSource = EditSupPos.ItemsSource = positions;

            CmbSpecForGroup.ItemsSource = await _adminService.GetAllSpecialtiesAsync();

            var orgs = await _adminService.GetAllOrganizationsAsync();
            CmbOrganizations.ItemsSource = CmbFilterTopicOrg.ItemsSource = orgs;

            RefreshStudentList();
            RefreshSupervisorList();
            RefreshPracticeTab();
            StructMenu_Checked(null, null);
            LoadLogs();
        }

        private bool IsValidEmail(string email) => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        // --- STUDENTS ---
        private async void RefreshStudentList(int? groupId = null)
        {
            var list = await _adminService.GetStudentsByGroupAsync(groupId);
            GridAllStudents.ItemsSource = new ObservableCollection<Student>(list);
        }
        private void CmbFilterGroup_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CmbFilterGroup.SelectedValue is int gid) RefreshStudentList(gid); }
        private void BtnResetStudentFilter_Click(object sender, RoutedEventArgs e) { CmbFilterGroup.SelectedIndex = -1; RefreshStudentList(); }

        private async void BtnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbStudGroup.SelectedValue == null) throw new Exception("Оберіть групу");
                if (!IsValidEmail(TxtStudEmail.Text)) throw new Exception("Невірний Email");
                int gid = (int)CmbStudGroup.SelectedValue;
                var u = new User { LastName = TxtStudLast.Text, FirstName = TxtStudFirst.Text, Email = TxtStudEmail.Text, PasswordHash = BCrypt.Net.BCrypt.HashPassword(TxtStudPass.Text), RoleId = 2, IsPasswordChangeRequired = true };
                _context.Users.Add(u); await _context.SaveChangesAsync();
                _context.Students.Add(new Student { UserId = u.UserId, GroupId = gid, RecordBookNumber = TxtStudRecord.Text });
                await _context.SaveChangesAsync();
                MessageBox.Show("Студента додано!"); RefreshStudentList(gid);
                TxtStudLast.Clear(); TxtStudFirst.Clear(); TxtStudEmail.Clear(); TxtStudPass.Clear(); TxtStudRecord.Clear();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // --- IMPORT ---
        private void BtnToggleImport_Click(object sender, RoutedEventArgs e) => PanelImport.Visibility = (PanelImport.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
        private void BtnCancelImport_Click(object sender, RoutedEventArgs e) => PanelImport.Visibility = Visibility.Collapsed;

        private void BtnProcessImport_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtRawNames.Text)) return;
            _importList = new List<StudentImportModel>();
            var lines = TxtRawNames.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string domain = TxtImportDomain.Text.Trim();
            long lastRbNum = GetNextRecordBookNumber();

            foreach (var l in lines)
            {
                var parts = l.Trim().Split(' ');
                if (parts.Length >= 2)
                {
                    string f = parts[0], l_name = parts[1];
                    string email = $"{Transliterate(f)}.{Transliterate(l_name)}{domain}".ToLower();
                    string rb = lastRbNum.ToString(); lastRbNum++;
                    _importList.Add(new StudentImportModel { FirstName = f, LastName = l_name, Email = email, RecordBook = rb });
                }
            }
            GridImportPreview.ItemsSource = _importList;
        }

        private long GetNextRecordBookNumber()
        {
            var maxRb = _context.Students.AsEnumerable().Select(s => long.TryParse(s.RecordBookNumber, out long n) ? n : 0).DefaultIfEmpty(0).Max();
            if (maxRb > 0) return maxRb + 1;
            string yearPrefix = DateTime.Now.Year.ToString().Substring(2);
            return long.Parse(yearPrefix + "0001");
        }

        private async void BtnSaveImport_Click(object sender, RoutedEventArgs e)
        {
            if (CmbImportGroup.SelectedValue == null) { MessageBox.Show("Оберіть групу!"); return; }
            int gid = (int)CmbImportGroup.SelectedValue; string pass = BCrypt.Net.BCrypt.HashPassword("123456");
            foreach (var item in _importList)
            {
                var u = new User { FirstName = item.FirstName, LastName = item.LastName, Email = item.Email, PasswordHash = pass, RoleId = 2, IsPasswordChangeRequired = true };
                _context.Users.Add(u); await _context.SaveChangesAsync();
                _context.Students.Add(new Student { UserId = u.UserId, GroupId = gid, RecordBookNumber = item.RecordBook });
            }
            await _context.SaveChangesAsync(); MessageBox.Show("Імпортовано!"); PanelImport.Visibility = Visibility.Collapsed; RefreshStudentList(gid);
        }

        // --- EDIT STUDENT ---
        private async void BtnEditStudent_Click(object sender, RoutedEventArgs e) { if (sender is Button btn && btn.Tag is int studentId) { var s = await _context.Students.Include(x => x.User).FirstOrDefaultAsync(x => x.StudentId == studentId); if (s == null) return; _editingId = studentId; EditStudLast.Text = s.User.LastName; EditStudFirst.Text = s.User.FirstName; EditStudEmail.Text = s.User.Email; EditStudRecord.Text = s.RecordBookNumber; EditStudGroup.SelectedValue = s.GroupId; ModalEditStudent.Visibility = Visibility.Visible; } }
        private async void BtnSaveEditStudent_Click(object sender, RoutedEventArgs e) { var s = await _context.Students.Include(x => x.User).FirstOrDefaultAsync(x => x.StudentId == _editingId); if (s != null) { s.User.LastName = EditStudLast.Text; s.User.FirstName = EditStudFirst.Text; s.User.Email = EditStudEmail.Text; s.RecordBookNumber = EditStudRecord.Text; if (EditStudGroup.SelectedValue != null) s.GroupId = (int)EditStudGroup.SelectedValue; await _context.SaveChangesAsync(); RefreshStudentList(); } ModalEditStudent.Visibility = Visibility.Collapsed; }
        private async void BtnDeleteStudent_Click(object sender, RoutedEventArgs e) { if (MessageBox.Show("Видалити?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes) { if (sender is Button btn && btn.Tag is int userId) { var s = await _context.Students.FirstOrDefaultAsync(x => x.UserId == userId); if (s != null) _context.Students.Remove(s); var u = await _context.Users.FindAsync(userId); if (u != null) _context.Users.Remove(u); await _context.SaveChangesAsync(); RefreshStudentList((int?)CmbFilterGroup.SelectedValue); } } }

        // --- SUPERVISORS ---
        private async void RefreshSupervisorList(int? deptId = null) { var list = await _adminService.GetSupervisorsByDeptAsync(deptId); GridAllSupervisors.ItemsSource = new ObservableCollection<Supervisor>(list); }
        private void CmbFilterDept_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CmbFilterDept.SelectedValue is int did) RefreshSupervisorList(did); }
        private void BtnResetSupervisorFilter_Click(object sender, RoutedEventArgs e) { CmbFilterDept.SelectedIndex = -1; RefreshSupervisorList(); }
        private async void BtnAddSupervisor_Click(object sender, RoutedEventArgs e) { try { if (CmbSupDept.SelectedValue == null) throw new Exception("Оберіть кафедру"); if (!IsValidEmail(TxtSupEmail.Text)) throw new Exception("Невірний Email"); int deptId = (int)CmbSupDept.SelectedValue; int? posId = CmbSupPos.SelectedValue as int?; var u = new User { LastName = TxtSupLast.Text, FirstName = TxtSupFirst.Text, Email = TxtSupEmail.Text, PasswordHash = BCrypt.Net.BCrypt.HashPassword(TxtSupPass.Text), RoleId = 3, IsPasswordChangeRequired = true }; _context.Users.Add(u); await _context.SaveChangesAsync(); _context.Supervisors.Add(new Supervisor { UserId = u.UserId, DepartmentId = deptId, PositionId = posId, Phone = TxtSupPhone.Text }); await _context.SaveChangesAsync(); MessageBox.Show("Керівника додано!"); RefreshSupervisorList(deptId); TxtSupLast.Clear(); TxtSupFirst.Clear(); TxtSupEmail.Clear(); TxtSupPass.Clear(); TxtSupPhone.Clear(); } catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); } }
        private async void BtnEditSupervisor_Click(object sender, RoutedEventArgs e) { if (sender is Button btn && btn.Tag is int supId) { var s = await _context.Supervisors.Include(x => x.User).FirstOrDefaultAsync(x => x.SupervisorId == supId); if (s == null) return; _editingId = supId; EditSupLast.Text = s.User.LastName; EditSupFirst.Text = s.User.FirstName; EditSupEmail.Text = s.User.Email; EditSupPhone.Text = s.Phone; EditSupDept.SelectedValue = s.DepartmentId; EditSupPos.SelectedValue = s.PositionId; ModalEditSupervisor.Visibility = Visibility.Visible; } }
        private async void BtnSaveEditSupervisor_Click(object sender, RoutedEventArgs e) { var s = await _context.Supervisors.Include(x => x.User).FirstOrDefaultAsync(x => x.SupervisorId == _editingId); if (s != null) { s.User.LastName = EditSupLast.Text; s.User.FirstName = EditSupFirst.Text; s.User.Email = EditSupEmail.Text; s.Phone = EditSupPhone.Text; if (EditSupDept.SelectedValue != null) s.DepartmentId = (int)EditSupDept.SelectedValue; if (EditSupPos.SelectedValue != null) s.PositionId = (int)EditSupPos.SelectedValue; await _context.SaveChangesAsync(); RefreshSupervisorList(); } ModalEditSupervisor.Visibility = Visibility.Collapsed; }
        private async void BtnDeleteSupervisor_Click(object sender, RoutedEventArgs e) { if (MessageBox.Show("Видалити?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes) { if (sender is Button btn && btn.Tag is int userId) { var s = await _context.Supervisors.FirstOrDefaultAsync(x => x.UserId == userId); if (s != null) _context.Supervisors.Remove(s); var u = await _context.Users.FindAsync(userId); if (u != null) _context.Users.Remove(u); await _context.SaveChangesAsync(); RefreshSupervisorList(); } } }

        // --- STRUCTURE ---
        private void StructMenu_Checked(object sender, RoutedEventArgs e) { if (PanelAddSpecialty == null) return; PanelAddSpecialty.Visibility = Visibility.Collapsed; GridSpecialties.Visibility = Visibility.Collapsed; PanelAddGroup.Visibility = Visibility.Collapsed; GridGroups.Visibility = Visibility.Collapsed; PanelAddDept.Visibility = Visibility.Collapsed; GridDepartments.Visibility = Visibility.Collapsed; PanelAddSimple.Visibility = Visibility.Collapsed; GridSimple.Visibility = Visibility.Collapsed; if (RbSpecialties.IsChecked == true) { TxtStructTitle.Text = "Спеціальності"; PanelAddSpecialty.Visibility = Visibility.Visible; GridSpecialties.Visibility = Visibility.Visible; LoadSpecialties(); } else if (RbGroups.IsChecked == true) { TxtStructTitle.Text = "Групи"; PanelAddGroup.Visibility = Visibility.Visible; GridGroups.Visibility = Visibility.Visible; LoadGroups(); } else if (RbDepartments.IsChecked == true) { TxtStructTitle.Text = "Кафедри"; PanelAddDept.Visibility = Visibility.Visible; GridDepartments.Visibility = Visibility.Visible; LoadDepts(); } else { TxtStructTitle.Text = (RbDisciplines.IsChecked == true) ? "Дисципліни" : "Посади"; PanelAddSimple.Visibility = Visibility.Visible; GridSimple.Visibility = Visibility.Visible; LoadSimple(); } }
        private async void LoadSpecialties() => GridSpecialties.ItemsSource = await _adminService.GetAllSpecialtiesAsync();
        private async void LoadGroups() => GridGroups.ItemsSource = await _adminService.GetAllGroupsAsync();
        private async void LoadDepts() => GridDepartments.ItemsSource = await _adminService.GetAllDepartmentsAsync();
        private async void LoadSimple() { var list = new List<SimpleItemViewModel>(); if (RbDisciplines.IsChecked == true) { var items = await _adminService.GetAllDisciplinesAsync(); list.AddRange(items.Select(x => new SimpleItemViewModel { Id = x.DisciplineId, Name = x.DisciplineName, Type = "Discipline" })); } else { var items = await _adminService.GetAllPositionsAsync(); list.AddRange(items.Select(x => new SimpleItemViewModel { Id = x.PositionId, Name = x.PositionName, Type = "Position" })); } GridSimple.ItemsSource = list; }
        private async void BtnAddSpecialty_Click(object sender, RoutedEventArgs e) { await _adminService.AddSpecialtyAsync(new Specialty { Code = TxtSpecCode.Text, Name = TxtSpecName.Text }); LoadSpecialties(); LoadAllData(); }
        private async void BtnAddGroup_Click(object sender, RoutedEventArgs e) { if (CmbSpecForGroup.SelectedValue == null) return; int y = int.TryParse(TxtGroupYear.Text, out int r) ? r : DateTime.Now.Year; await _adminService.AddGroupAsync(new StudentGroup { GroupCode = TxtGroupCode.Text, SpecialtyId = (int)CmbSpecForGroup.SelectedValue, EntryYear = y }); LoadGroups(); LoadAllData(); }
        private async void BtnAddDept_Click(object sender, RoutedEventArgs e) { await _adminService.AddDepartmentAsync(new Department { DepartmentName = TxtDeptName.Text }); LoadDepts(); LoadAllData(); }
        private async void BtnAddSimple_Click(object sender, RoutedEventArgs e) { if (RbDisciplines.IsChecked == true) await _adminService.AddDisciplineAsync(new Discipline { DisciplineName = TxtSimpleName.Text }); else await _adminService.AddPositionAsync(new Position { PositionName = TxtSimpleName.Text }); LoadSimple(); }
        private async void BtnDeleteStruct_Click(object sender, RoutedEventArgs e) { if (MessageBox.Show("Видалити?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes) { if (sender is Button btn && btn.Tag != null) { try { if (btn.Tag is Specialty s) await _adminService.DeleteSpecialtyAsync(s.SpecialtyId); else if (btn.Tag is StudentGroup g) await _adminService.DeleteGroupAsync(g.GroupId); else if (btn.Tag is Department d) await _adminService.DeleteDepartmentAsync(d.DepartmentId); else if (btn.Tag is SimpleItemViewModel sim) { if (sim.Type == "Discipline") await _adminService.DeleteDisciplineAsync(sim.Id); else await _adminService.DeletePositionAsync(sim.Id); } StructMenu_Checked(null, null); LoadAllData(); } catch (Exception ex) { MessageBox.Show(ex.Message); } } } }
        private void BtnEditStruct_Click(object sender, RoutedEventArgs e) { if (sender is Button btn && btn.Tag != null) { _editingItem = btn.Tag; PanelEditStructCode.Visibility = Visibility.Collapsed; PanelEditStructYear.Visibility = Visibility.Collapsed; if (_editingItem is Specialty s) { LblStructName.Text = "Назва:"; EditStructName.Text = s.Name; LblStructCode.Text = "Код:"; EditStructCode.Text = s.Code; PanelEditStructCode.Visibility = Visibility.Visible; } else if (_editingItem is StudentGroup g) { LblStructName.Text = "Шифр:"; EditStructName.Text = g.GroupCode; EditStructYear.Text = g.EntryYear.ToString(); PanelEditStructYear.Visibility = Visibility.Visible; } else if (_editingItem is Department d) { LblStructName.Text = "Назва:"; EditStructName.Text = d.DepartmentName; } else if (_editingItem is SimpleItemViewModel sim) { LblStructName.Text = "Назва:"; EditStructName.Text = sim.Name; } ModalEditStruct.Visibility = Visibility.Visible; } }
        private async void BtnSaveEditStruct_Click(object sender, RoutedEventArgs e) { if (_editingItem is Specialty s) { s.Name = EditStructName.Text; s.Code = EditStructCode.Text; } else if (_editingItem is StudentGroup g) { g.GroupCode = EditStructName.Text; if (int.TryParse(EditStructYear.Text, out int y)) g.EntryYear = y; } else if (_editingItem is Department d) { d.DepartmentName = EditStructName.Text; } else if (_editingItem is SimpleItemViewModel sim) { if (sim.Type == "Discipline") { var disc = await _context.Disciplines.FindAsync(sim.Id); if (disc != null) disc.DisciplineName = EditStructName.Text; } else { var p = await _context.Positions.FindAsync(sim.Id); if (p != null) p.PositionName = EditStructName.Text; } } await _context.SaveChangesAsync(); ModalEditStruct.Visibility = Visibility.Collapsed; StructMenu_Checked(null, null); }

        // --- PRACTICE ---
        private async void RefreshPracticeTab(int? orgId = null, string type = "Всі")
        {
            var topics = await _practiceService.GetAvailableTopicsAsync();
            if (orgId.HasValue) topics = topics.Where(t => t.OrganizationId == orgId.Value);
            GridTopics.ItemsSource = new ObservableCollection<InternshipTopic>(topics);

            var orgs = await _adminService.GetAllOrganizationsAsync();
            if (type != "Всі") orgs = orgs.Where(o => o.Type == type);
            GridOrgs.ItemsSource = new ObservableCollection<Organization>(orgs);
        }
        private void CmbFilterTopicOrg_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CmbFilterTopicOrg.SelectedValue is int oid) RefreshPracticeTab(oid, (CmbFilterOrgType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Всі"); }
        private void CmbFilterOrgType_SelectionChanged(object sender, SelectionChangedEventArgs e) { string t = (CmbFilterOrgType.SelectedItem as ComboBoxItem)?.Content.ToString(); RefreshPracticeTab((int?)CmbFilterTopicOrg.SelectedValue, t); }
        private void BtnResetTopicFilter_Click(object sender, RoutedEventArgs e) { CmbFilterTopicOrg.SelectedIndex = -1; RefreshPracticeTab(null, (CmbFilterOrgType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Всі"); }
        private void BtnResetOrgFilter_Click(object sender, RoutedEventArgs e) { CmbFilterOrgType.SelectedIndex = 0; RefreshPracticeTab((int?)CmbFilterTopicOrg.SelectedValue, "Всі"); }

        private async void BtnAddTopic_Click(object sender, RoutedEventArgs e) { if (CmbOrganizations.SelectedValue == null) return; await _practiceService.AddTopicAsync(new InternshipTopic { Title = TxtTopicTitle.Text, Description = TxtTopicDesc.Text, OrganizationId = (int)CmbOrganizations.SelectedValue, IsAvailable = true }); RefreshPracticeTab(); }
        private async void BtnAddOrg_Click(object sender, RoutedEventArgs e) { string type = CmbOrgType.SelectedIndex == 0 ? "External" : "University"; await _adminService.AddOrganizationAsync(new Organization { Name = TxtOrgName.Text, Address = TxtOrgAddr.Text, Type = type, ContactEmail = "" }); RefreshPracticeTab(); LoadAllData(); }

        private async void ChkTopicAvailable_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk && chk.DataContext is InternshipTopic t)
            {
                var dbTopic = await _context.InternshipTopics.FindAsync(t.TopicId);
                if (dbTopic != null) { dbTopic.IsAvailable = chk.IsChecked ?? false; await _context.SaveChangesAsync(); }
            }
        }

        // --- LOGS ---
        private void LoadLogs() { GridLogs.ItemsSource = new ObservableCollection<AuditLog>(_context.AuditLogs.OrderByDescending(x => x.TimeStamp).ToList()); }
        private void BtnRefreshLogs_Click(object sender, RoutedEventArgs e) => LoadLogs();

        // --- HELPERS ---
        private void BtnLogout_Click(object sender, RoutedEventArgs e) { new LoginWindow().Show(); Close(); }
        private void BtnCloseModal_Click(object sender, RoutedEventArgs e) { ModalEditStudent.Visibility = ModalEditSupervisor.Visibility = ModalEditStruct.Visibility = Visibility.Collapsed; }

        private string Transliterate(string s)
        {
            string[] ukr = { "а", "б", "в", "г", "ґ", "д", "е", "є", "ж", "з", "и", "і", "ї", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ь", "ю", "я" };
            string[] lat = { "a", "b", "v", "h", "g", "d", "e", "ye", "zh", "z", "y", "i", "yi", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "kh", "ts", "ch", "sh", "shch", "", "yu", "ya" };
            StringBuilder sb = new StringBuilder();
            foreach (char c in s.ToLower()) { int idx = Array.IndexOf(ukr, c.ToString()); if (idx >= 0) sb.Append(lat[idx]); else if (char.IsLetterOrDigit(c)) sb.Append(c); }
            return sb.ToString();
        }
    }
}