using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Practice.Data.Entities;
using Practice.Services.Interfaces;

namespace Practice.Windows
{
    public class StudentImportModel { public string FirstName { get; set; } public string LastName { get; set; } public string Email { get; set; } public string RecordBook { get; set; } }
    public class SimpleItemViewModel { public int Id { get; set; } public string Name { get; set; } public string Type { get; set; } }

    public partial class AdminWindow : Window
    {
        private readonly User _currentUser;
        private readonly IAdminService _adminService;
        private readonly IIdentityService _identityService;
        private readonly IPracticeService _practiceService;
        private readonly ICourseService _courseService;
        private readonly IAuditService _auditService;

        private List<AuditLog> _allLogs = new List<AuditLog>();
        private List<StudentImportModel> _importList;
        private int _editingId;
        private object _editingItem;

        public AdminWindow(User user, IAdminService adminService, IIdentityService identityService, IPracticeService practiceService, ICourseService courseService, IAuditService auditService)
        {
            InitializeComponent();
            _currentUser = user;
            _adminService = adminService;
            _identityService = identityService;
            _practiceService = practiceService;
            _courseService = courseService;
            _auditService = auditService;

            TxtAdminName.Text = $"{_currentUser.LastName} {_currentUser.FirstName}";
            TxtInitials.Text = (!string.IsNullOrEmpty(_currentUser.FirstName) ? _currentUser.FirstName[0].ToString() : "A");

            LoadAllData();
        }

        private void LoadAllData()
        {
            try
            {
                ReloadAllDictionaries();
                RefreshStudentList();
                RefreshSupervisorList();
                RefreshPracticeTab();
                RefreshCoursesList();
                StructMenu_Checked(null, null);
                LoadLogs();
            }
            catch (Exception ex) { MessageBox.Show($"Помилка ініціалізації: {ex.Message}"); }
        }

        private async void ReloadAllDictionaries()
        {
            var groups = await _identityService.GetAllGroupsAsync();
            CmbStudGroup.ItemsSource = CmbImportGroup.ItemsSource = CmbFilterGroup.ItemsSource = EditStudGroup.ItemsSource = CmbCourseFilterGroup.ItemsSource = CmbEnrollGroup.ItemsSource = groups;

            var depts = await _identityService.GetAllDepartmentsAsync();
            CmbSupDept.ItemsSource = CmbFilterDept.ItemsSource = EditSupDept.ItemsSource = CmbDeptForGroup.ItemsSource = CmbDeptForSpec.ItemsSource = EditStructDept.ItemsSource = CmbOrgDeptSource.ItemsSource = depts;

            var positions = await _adminService.GetAllPositionsAsync();
            CmbSupPos.ItemsSource = EditSupPos.ItemsSource = positions;

            var disciplines = await _courseService.GetAllDisciplinesAsync();
            CmbCourseDiscipline.ItemsSource = CmbTopicDiscipline.ItemsSource = EditCourseDiscipline.ItemsSource = disciplines;

            var specialties = await _adminService.GetAllSpecialtiesAsync();
            CmbSpecForGroup.ItemsSource = EditStructSpec.ItemsSource = specialties;

            var orgs = await _practiceService.GetAllOrganizationsAsync();
            CmbOrganizations.ItemsSource = CmbFilterTopicOrg.ItemsSource = EditTopicOrg.ItemsSource = orgs;

            ReloadCourseSupervisors();
            ReloadActiveCourses();
        }

        private async void ReloadCourseSupervisors()
        {
            var sups = await _adminService.GetSupervisorsByDeptAsync(null);
            CmbCourseSupervisor.ItemsSource = EditCourseSupervisor.ItemsSource = sups;
        }
        private async void ReloadActiveCourses() => CmbEnrollCourse.ItemsSource = await _courseService.GetAllActiveCoursesAsync();
        private bool IsValidEmail(string email) => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private async Task TryDelete(Func<Task> deleteAction, string errorMessage = "Неможливо видалити запис.")
        {
            try
            {
                await deleteAction();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FOREIGN KEY") || (ex.InnerException != null && ex.InnerException.Message.Contains("FOREIGN KEY")))
                {
                    MessageBox.Show($"{errorMessage}\n\nПричина: Цей запис використовується в інших таблицях (має залежні дані).", "Помилка видалення", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"Помилка БД: {ex.Message}");
                }
            }
        }

        private async void RefreshStudentList(int? groupId = null) => GridAllStudents.ItemsSource = new ObservableCollection<Student>(await _adminService.GetStudentsByGroupAsync(groupId));
        private void CmbFilterGroup_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CmbFilterGroup.SelectedValue is int gid) RefreshStudentList(gid); }
        private void BtnResetStudentFilter_Click(object sender, RoutedEventArgs e) { CmbFilterGroup.SelectedIndex = -1; RefreshStudentList(); }

        private async void BtnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbStudGroup.SelectedValue == null) throw new Exception("Оберіть групу");
                if (!IsValidEmail(TxtStudEmail.Text)) throw new Exception("Невірний Email");
                int gid = (int)CmbStudGroup.SelectedValue;
                var u = new User { LastName = TxtStudLast.Text, FirstName = TxtStudFirst.Text, Email = TxtStudEmail.Text, RoleId = 2 };
                await _identityService.RegisterStudentAsync(u, TxtStudPass.Text, gid, TxtStudRecord.Text);
                MessageBox.Show("Студента додано!");
                RefreshStudentList((int?)CmbFilterGroup.SelectedValue); LoadLogs();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void BtnDeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int userId)
                if (MessageBox.Show("Видалити студента?", "Підтвердження", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    await TryDelete(async () => {
                        await _adminService.DeleteStudentAsync(userId);
                        RefreshStudentList((int?)CmbFilterGroup.SelectedValue);
                        LoadLogs();
                    }, "Не вдалося видалити студента.");
        }

        private void BtnToggleImport_Click(object sender, RoutedEventArgs e) => PanelImport.Visibility = (PanelImport.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
        private void BtnCancelImport_Click(object sender, RoutedEventArgs e) => PanelImport.Visibility = Visibility.Collapsed;
        private void BtnProcessImport_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtRawNames.Text)) return;
            _importList = new List<StudentImportModel>();
            var lines = TxtRawNames.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string domain = TxtImportDomain.Text.Trim();
            long lastRbNum = DateTime.Now.Ticks % 100000;
            foreach (var l in lines)
            {
                var parts = l.Trim().Split(' ');
                if (parts.Length >= 2)
                {
                    string f = parts[0], l_name = parts[1];
                    string email = $"{Transliterate(f)}.{Transliterate(l_name)}{domain}".ToLower();
                    _importList.Add(new StudentImportModel { FirstName = f, LastName = l_name, Email = email, RecordBook = (lastRbNum++).ToString() });
                }
            }
            GridImportPreview.ItemsSource = _importList;
        }
        private async void BtnSaveImport_Click(object sender, RoutedEventArgs e)
        {
            if (CmbImportGroup.SelectedValue == null) { MessageBox.Show("Оберіть групу!"); return; }
            int gid = (int)CmbImportGroup.SelectedValue;
            try
            {
                foreach (var item in _importList)
                {
                    var u = new User { FirstName = item.FirstName, LastName = item.LastName, Email = item.Email, RoleId = 2 };
                    await _identityService.RegisterStudentAsync(u, "123456", gid, item.RecordBook);
                }
                MessageBox.Show("Імпортовано!"); PanelImport.Visibility = Visibility.Collapsed; RefreshStudentList(gid); LoadLogs();
            }
            catch (Exception ex) { MessageBox.Show("Помилка імпорту: " + ex.Message); }
        }

        private async void BtnEditStudent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int studentId)
            {
                var s = await _adminService.GetStudentByIdAsync(studentId);
                if (s == null) return;
                _editingId = studentId;
                EditStudLast.Text = s.User.LastName; EditStudFirst.Text = s.User.FirstName; EditStudEmail.Text = s.User.Email; EditStudRecord.Text = s.RecordBookNumber; EditStudGroup.SelectedValue = s.GroupId;
                ModalEditStudent.Visibility = Visibility.Visible;
            }
        }
        private async void BtnSaveEditStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _adminService.UpdateStudentAsync(_editingId, EditStudFirst.Text, EditStudLast.Text, EditStudEmail.Text, EditStudRecord.Text, (int)EditStudGroup.SelectedValue);
                RefreshStudentList((int?)CmbFilterGroup.SelectedValue); ModalEditStudent.Visibility = Visibility.Collapsed; LoadLogs();
            }
            catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
        }

        private async void RefreshSupervisorList(int? deptId = null) => GridAllSupervisors.ItemsSource = new ObservableCollection<Supervisor>(await _adminService.GetSupervisorsByDeptAsync(deptId));
        private void CmbFilterDept_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CmbFilterDept.SelectedValue is int did) RefreshSupervisorList(did); }
        private void BtnResetSupervisorFilter_Click(object sender, RoutedEventArgs e) { CmbFilterDept.SelectedIndex = -1; RefreshSupervisorList(); }

        private async void BtnAddSupervisor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbSupDept.SelectedValue == null) throw new Exception("Оберіть кафедру");
                var u = new User { LastName = TxtSupLast.Text, FirstName = TxtSupFirst.Text, Email = TxtSupEmail.Text, RoleId = 3 };
                await _identityService.RegisterSupervisorAsync(u, TxtSupPass.Text, (int)CmbSupDept.SelectedValue, CmbSupPos.SelectedValue as int?, TxtSupPhone.Text);
                MessageBox.Show("Керівника додано!");
                RefreshSupervisorList((int?)CmbFilterDept.SelectedValue); LoadLogs();
            }
            catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
        }

        private async void BtnDeleteSupervisor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int userId)
                if (MessageBox.Show("Видалити?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    await TryDelete(async () => {
                        await _adminService.DeleteSupervisorAsync(userId);
                        RefreshSupervisorList((int?)CmbFilterDept.SelectedValue);
                        LoadLogs();
                    }, "Не вдалося видалити керівника.");
        }

        private async void BtnEditSupervisor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int supId)
            {
                var s = await _adminService.GetSupervisorByIdAsync(supId);
                _editingId = supId;
                EditSupLast.Text = s.User.LastName; EditSupFirst.Text = s.User.FirstName; EditSupEmail.Text = s.User.Email; EditSupPhone.Text = s.Phone;
                EditSupDept.SelectedValue = s.DepartmentId; EditSupPos.SelectedValue = s.PositionId;
                ModalEditSupervisor.Visibility = Visibility.Visible;
            }
        }
        private async void BtnSaveEditSupervisor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _adminService.UpdateSupervisorAsync(_editingId, EditSupFirst.Text, EditSupLast.Text, EditSupEmail.Text, EditSupPhone.Text, (int)EditSupDept.SelectedValue, (int?)EditSupPos.SelectedValue);
                RefreshSupervisorList((int?)CmbFilterDept.SelectedValue); ModalEditSupervisor.Visibility = Visibility.Collapsed; LoadLogs();
            }
            catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
        }

        private void StructMenu_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelAddSpecialty == null) return;
            PanelAddSpecialty.Visibility = GridSpecialties.Visibility = Visibility.Collapsed;
            PanelAddGroup.Visibility = GridGroups.Visibility = Visibility.Collapsed;
            PanelAddDept.Visibility = GridDepartments.Visibility = Visibility.Collapsed;
            PanelAddSimple.Visibility = GridSimple.Visibility = Visibility.Collapsed;

            if (RbSpecialties.IsChecked == true) { TxtStructTitle.Text = "Спеціальності"; PanelAddSpecialty.Visibility = Visibility.Visible; GridSpecialties.Visibility = Visibility.Visible; LoadSpecialties(); }
            else if (RbGroups.IsChecked == true) { TxtStructTitle.Text = "Групи"; PanelAddGroup.Visibility = Visibility.Visible; GridGroups.Visibility = Visibility.Visible; LoadGroups(); }
            else if (RbDepartments.IsChecked == true) { TxtStructTitle.Text = "Кафедри"; PanelAddDept.Visibility = Visibility.Visible; GridDepartments.Visibility = Visibility.Visible; LoadDepts(); }
            else { TxtStructTitle.Text = (RbDisciplines.IsChecked == true) ? "Дисципліни" : "Посади"; PanelAddSimple.Visibility = Visibility.Visible; GridSimple.Visibility = Visibility.Visible; LoadSimple(); }
        }

        private async void LoadSpecialties() => GridSpecialties.ItemsSource = await _adminService.GetAllSpecialtiesAsync();
        private async void LoadGroups() => GridGroups.ItemsSource = await _adminService.GetAllGroupsAsync();
        private async void LoadDepts() => GridDepartments.ItemsSource = await _identityService.GetAllDepartmentsAsync();
        private async void LoadSimple()
        {
            var list = new List<SimpleItemViewModel>();
            if (RbDisciplines.IsChecked == true) { var items = await _courseService.GetAllDisciplinesAsync(); list.AddRange(items.Select(x => new SimpleItemViewModel { Id = x.DisciplineId, Name = x.DisciplineName, Type = "Discipline" })); }
            else { var items = await _adminService.GetAllPositionsAsync(); list.AddRange(items.Select(x => new SimpleItemViewModel { Id = x.PositionId, Name = x.PositionName, Type = "Position" })); }
            GridSimple.ItemsSource = list;
        }

        private async void BtnAddSpecialty_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbDeptForSpec.SelectedValue == null) throw new Exception("Оберіть кафедру");
                await _adminService.AddSpecialtyAsync(new Specialty { Code = TxtSpecCode.Text, Name = TxtSpecName.Text, DepartmentId = (int)CmbDeptForSpec.SelectedValue });
                LoadSpecialties();
                ReloadAllDictionaries();
                LoadLogs();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        private async void BtnAddGroup_Click(object sender, RoutedEventArgs e) { try { if (CmbSpecForGroup.SelectedValue == null) return; int y = int.TryParse(TxtGroupYear.Text, out int r) ? r : DateTime.Now.Year; await _adminService.AddGroupAsync(new StudentGroup { GroupCode = TxtGroupCode.Text, SpecialtyId = (int)CmbSpecForGroup.SelectedValue, EntryYear = y }); LoadGroups(); LoadLogs(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
        private async void CmbDeptForGroup_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CmbDeptForGroup.SelectedValue is int deptId) { var allSpecs = await _adminService.GetAllSpecialtiesAsync(); CmbSpecForGroup.ItemsSource = allSpecs.Where(s => s.DepartmentId == deptId).ToList(); if (CmbSpecForGroup.Items.Count > 0) CmbSpecForGroup.SelectedIndex = 0; } }
        private async void BtnAddDept_Click(object sender, RoutedEventArgs e) { try { await _adminService.AddDepartmentAsync(new Department { DepartmentName = TxtDeptName.Text }); LoadDepts(); ReloadAllDictionaries(); LoadLogs(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
        private async void BtnAddSimple_Click(object sender, RoutedEventArgs e) { try { if (RbDisciplines.IsChecked == true) await _courseService.AddDisciplineAsync(TxtSimpleName.Text); else await _adminService.AddPositionAsync(new Position { PositionName = TxtSimpleName.Text }); LoadSimple(); ReloadAllDictionaries(); LoadLogs(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }

        private async void BtnDeleteStruct_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Видалити?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (sender is Button btn && btn.Tag != null)
                {
                    await TryDelete(async () => {
                        if (btn.Tag is Specialty s) await _adminService.DeleteSpecialtyAsync(s.SpecialtyId);
                        else if (btn.Tag is StudentGroup g) await _adminService.DeleteGroupAsync(g.GroupId);
                        else if (btn.Tag is Department d) await _adminService.DeleteDepartmentAsync(d.DepartmentId);
                        else if (btn.Tag is SimpleItemViewModel sim)
                        {
                            if (sim.Type == "Discipline") await _adminService.DeleteDisciplineAsync(sim.Id);
                            else await _adminService.DeletePositionAsync(sim.Id);
                        }
                        StructMenu_Checked(null, null); ReloadAllDictionaries(); LoadLogs();
                    }, "Неможливо видалити запис структури.");
                }
            }
        }

        private void BtnEditStruct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                _editingItem = btn.Tag;
                PanelEditStructCode.Visibility = PanelEditStructYear.Visibility = PanelEditStructDept.Visibility = PanelEditStructSpec.Visibility = Visibility.Collapsed;

                if (_editingItem is Specialty s)
                {
                    LblStructName.Text = "Назва:"; EditStructName.Text = s.Name;
                    LblStructCode.Text = "Код:"; EditStructCode.Text = s.Code;
                    EditStructDept.SelectedValue = s.DepartmentId;
                    PanelEditStructCode.Visibility = PanelEditStructDept.Visibility = Visibility.Visible;
                }
                else if (_editingItem is StudentGroup g)
                {
                    LblStructName.Text = "Шифр:"; EditStructName.Text = g.GroupCode;
                    EditStructYear.Text = g.EntryYear.ToString();
                    var deptId = g.Specialty.DepartmentId;
                    EditStructDept.SelectedValue = deptId;
                    PanelEditStructYear.Visibility = PanelEditStructDept.Visibility = PanelEditStructSpec.Visibility = Visibility.Visible;
                }
                else if (_editingItem is Department d)
                {
                    LblStructName.Text = "Назва:"; EditStructName.Text = d.DepartmentName;
                }
                ModalEditStruct.Visibility = Visibility.Visible;
            }
        }

        private async void EditStructDept_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EditStructDept.SelectedValue is int deptId)
            {
                var allSpecs = await _adminService.GetAllSpecialtiesAsync();
                EditStructSpec.ItemsSource = allSpecs.Where(s => s.DepartmentId == deptId).ToList();
                if (_editingItem is StudentGroup g && g.Specialty.DepartmentId == deptId) EditStructSpec.SelectedValue = g.SpecialtyId;
                else if (EditStructSpec.Items.Count > 0) EditStructSpec.SelectedIndex = 0;
            }
        }

        private async void BtnSaveEditStruct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_editingItem is Specialty s)
                {
                    s.Name = EditStructName.Text; s.Code = EditStructCode.Text;
                    if (EditStructDept.SelectedValue is int did) s.DepartmentId = did;
                    await _adminService.UpdateSpecialtyAsync(s);
                }
                else if (_editingItem is StudentGroup g)
                {
                    g.GroupCode = EditStructName.Text;
                    if (int.TryParse(EditStructYear.Text, out int y)) g.EntryYear = y;
                    if (EditStructSpec.SelectedValue is int sid) g.SpecialtyId = sid;
                    await _adminService.UpdateGroupAsync(g);
                }
                else if (_editingItem is Department d)
                {
                    d.DepartmentName = EditStructName.Text;
                    await _adminService.UpdateDepartmentAsync(d);
                }
                ModalEditStruct.Visibility = Visibility.Collapsed;
                StructMenu_Checked(null, null); ReloadAllDictionaries(); LoadLogs();
            }
            catch (Exception ex) { MessageBox.Show("Помилка оновлення: " + ex.Message); }
        }

        private async void RefreshPracticeTab(int? orgId = null, string type = null)
        {
            var topics = await _practiceService.GetAvailableTopicsAsync();
            if (orgId.HasValue) topics = topics.Where(t => t.OrganizationId == orgId.Value);
            GridTopics.ItemsSource = new ObservableCollection<InternshipTopic>(topics);

            var orgs = await _practiceService.GetAllOrganizationsAsync();
            if (!string.IsNullOrEmpty(type)) orgs = orgs.Where(o => o.Type == type);
            GridOrgs.ItemsSource = new ObservableCollection<Organization>(orgs);
        }
        private void CmbFilterTopicOrg_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (CmbFilterTopicOrg.SelectedValue is int oid) RefreshPracticeTab(oid, (CmbFilterOrgType.SelectedItem as ComboBoxItem)?.Content.ToString()); }
        private void CmbFilterOrgType_SelectionChanged(object sender, SelectionChangedEventArgs e) { string t = (CmbFilterOrgType.SelectedItem as ComboBoxItem)?.Content.ToString(); RefreshPracticeTab((int?)CmbFilterTopicOrg.SelectedValue, t); }
        private void BtnResetTopicFilter_Click(object sender, RoutedEventArgs e) { CmbFilterTopicOrg.SelectedIndex = -1; RefreshPracticeTab(null, (CmbFilterOrgType.SelectedItem as ComboBoxItem)?.Content.ToString()); }
        private void BtnResetOrgFilter_Click(object sender, RoutedEventArgs e) { CmbFilterOrgType.SelectedIndex = -1; RefreshPracticeTab((int?)CmbFilterTopicOrg.SelectedValue, null); }

        private void CmbOrgType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelOrgDeptSelect == null) return;
            var isUni = (CmbOrgType.SelectedItem as ComboBoxItem)?.Content.ToString() == "Кафедра";
            PanelOrgDeptSelect.Visibility = isUni ? Visibility.Visible : Visibility.Collapsed;
            PanelOrgNameInput.Visibility = isUni ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void BtnAddOrg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string type = CmbOrgType.SelectedIndex == 0 ? "External" : "University";
                string name = TxtOrgName.Text;
                if (type == "University")
                {
                    if (CmbOrgDeptSource.SelectedValue == null) throw new Exception("Оберіть кафедру!");
                    name = CmbOrgDeptSource.SelectedValue.ToString();
                }
                await _practiceService.CreateOrganizationAsync(name, TxtOrgAddr.Text, type);
                RefreshPracticeTab(); ReloadAllDictionaries(); LoadLogs();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void BtnAddTopic_Click(object sender, RoutedEventArgs e) { try { if (CmbOrganizations.SelectedValue == null) throw new Exception("Оберіть організацію"); int? discId = CmbTopicDiscipline.SelectedValue as int?; await _practiceService.AddTopicAsync(new InternshipTopic { Title = TxtTopicTitle.Text, Description = TxtTopicDesc.Text, OrganizationId = (int)CmbOrganizations.SelectedValue, DisciplineId = discId, IsAvailable = true }); RefreshPracticeTab(); LoadLogs(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }

        private async void BtnDeleteTopic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
                if (MessageBox.Show("Видалити тему?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    await TryDelete(async () => {
                        await _practiceService.DeleteTopicAsync(id);
                        RefreshPracticeTab();
                        LoadLogs();
                    });
        }

        private void BtnEditTopic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var topic = (GridTopics.ItemsSource as IEnumerable<InternshipTopic>)?.FirstOrDefault(t => t.TopicId == id);
                if (topic == null) return;
                _editingId = id;
                EditTopicTitle.Text = topic.Title;
                EditTopicDesc.Text = topic.Description;
                EditTopicOrg.SelectedValue = topic.OrganizationId;
                ModalEditTopic.Visibility = Visibility.Visible;
            }
        }

        private async void BtnSaveEditTopic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var t = new InternshipTopic { TopicId = _editingId, Title = EditTopicTitle.Text, Description = EditTopicDesc.Text, OrganizationId = (int)EditTopicOrg.SelectedValue };
                await _practiceService.UpdateTopicAsync(t);
                RefreshPracticeTab();
                ModalEditTopic.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
        }

        private async void BtnDeleteOrg_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
                if (MessageBox.Show("Видалити організацію?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    await TryDelete(async () => {
                        await _adminService.DeleteOrganizationAsync(id);
                        RefreshPracticeTab();
                        ReloadAllDictionaries();
                        LoadLogs();
                    });
        }

        private void BtnEditOrg_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var org = (GridOrgs.ItemsSource as IEnumerable<Organization>)?.FirstOrDefault(o => o.OrganizationId == id);
                if (org == null) return;
                _editingId = id;
                EditOrgName.Text = org.Name;
                EditOrgAddr.Text = org.Address;
                EditOrgType.SelectedIndex = org.Type == "External" ? 0 : 1;
                ModalEditOrg.Visibility = Visibility.Visible;
            }
        }

        private async void BtnSaveEditOrg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string type = EditOrgType.SelectedIndex == 0 ? "External" : "University";
                var org = new Organization { OrganizationId = _editingId, Name = EditOrgName.Text, Address = EditOrgAddr.Text, Type = type };
                await _adminService.UpdateOrganizationAsync(org);
                RefreshPracticeTab();
                ModalEditOrg.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
        }

        private async void RefreshCoursesList()
        {
            try
            {
                var allCourses = await _courseService.GetAllActiveCoursesAsync();

                GridCourses.ItemsSource = null;

                GridCourses.ItemsSource = new ObservableCollection<Course>(allCourses.ToList());

                // Оновлюємо випадайку курсів у вкладці зарахування
                CmbEnrollCourse.ItemsSource = allCourses;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка оновлення UI: " + ex.Message);
            }
        }
        private void BtnRefreshCourses_Click(object sender, RoutedEventArgs e) => RefreshCoursesList();
        private async void BtnAddCourse_Click(object sender, RoutedEventArgs e) { try { if (CmbCourseDiscipline.SelectedValue == null) throw new Exception("Оберіть дисципліну!"); var c = new Course { Name = TxtCourseName.Text, Year = int.Parse(TxtCourseYear.Text), DisciplineId = (int)CmbCourseDiscipline.SelectedValue, IsActive = true, SupervisorId = CmbCourseSupervisor.SelectedValue as int? }; await _courseService.CreateCourseAsync(c); MessageBox.Show("Курс створено!"); RefreshCoursesList(); ReloadActiveCourses(); LoadLogs(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
        private async void BtnEnrollGroup_Click(object sender, RoutedEventArgs e) { try { if (CmbEnrollCourse.SelectedValue == null || CmbEnrollGroup.SelectedValue == null) throw new Exception("Оберіть курс та групу!"); int courseId = (int)CmbEnrollCourse.SelectedValue; int groupId = (int)CmbEnrollGroup.SelectedValue; var students = await _adminService.GetStudentsByGroupAsync(groupId); foreach (var s in students) await _courseService.EnrollStudentToCourseAsync(s.StudentId, courseId, groupId); MessageBox.Show($"Зараховано!"); LoadLogs(); } catch (Exception ex) { MessageBox.Show(ex.Message); } }

        private void BtnEditCourse_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var c = (GridCourses.ItemsSource as IEnumerable<Course>)?.FirstOrDefault(x => x.CourseId == id);
                if (c == null) return;
                _editingId = id;
                EditCourseName.Text = c.Name;
                EditCourseYear.Text = c.Year.ToString();
                EditCourseDiscipline.SelectedValue = c.DisciplineId;
                EditCourseSupervisor.SelectedValue = c.SupervisorId;
                ModalEditCourse.Visibility = Visibility.Visible;
            }
        }

        private async void BtnSaveEditCourse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var courseToUpdate = new Course
                {
                    CourseId = _editingId,
                    Name = EditCourseName.Text,
                    Year = int.Parse(EditCourseYear.Text),
                    DisciplineId = (int)EditCourseDiscipline.SelectedValue,
                    SupervisorId = (int?)EditCourseSupervisor.SelectedValue,
                    IsActive = true
                };

                await _courseService.UpdateCourseAsync(courseToUpdate);

                await Task.Delay(150);

                ModalEditCourse.Visibility = Visibility.Collapsed;

                RefreshCoursesList();

                MessageBox.Show("Зміни успішно відображено!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void BtnDeleteCourse_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
                if (MessageBox.Show("Видалити курс?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    await TryDelete(async () => {
                        await _courseService.DeleteCourseAsync(id);
                        RefreshCoursesList();
                        LoadLogs();
                    });
        }

        // --- ЛОГИ ---
        private async void LoadLogs() { try { _allLogs = await _auditService.GetAllLogsAsync(); ApplyLogFilter(); } catch { } }
        private void CmbLogFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyLogFilter();
        private void BtnRefreshLogs_Click(object sender, RoutedEventArgs e) => LoadLogs();
        private void ApplyLogFilter()
        {
            if (_allLogs == null || CmbLogFilter == null || GridLogs == null) return;
            string tag = (CmbLogFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            var q = _allLogs.AsEnumerable();
            if (tag == "Auth") q = q.Where(l => l.Action.StartsWith("Auth") || l.Action == "Register");
            else if (tag == "Other") q = q.Where(l => !l.Action.Contains("Create") && !l.Action.Contains("Update") && !l.Action.Contains("Delete") && !l.Action.Contains("Auth"));
            else if (tag != "All") q = q.Where(l => l.Action.Contains(tag));
            GridLogs.ItemsSource = new ObservableCollection<AuditLog>(q);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e) { new LoginWindow().Show(); Close(); }
        private void BtnCloseModal_Click(object sender, RoutedEventArgs e) { ModalEditStudent.Visibility = ModalEditSupervisor.Visibility = ModalEditStruct.Visibility = ModalEditTopic.Visibility = ModalEditOrg.Visibility = ModalEditCourse.Visibility = Visibility.Collapsed; }
        private void BtnBackup_Click(object sender, RoutedEventArgs e) { SaveFileDialog d = new SaveFileDialog(); d.Filter = "DB|*.db"; if (d.ShowDialog() == true) try { _practiceService.CreateBackup(d.FileName); MessageBox.Show("Backup OK"); } catch (Exception ex) { MessageBox.Show(ex.Message); } }
        private string Transliterate(string s) { return s; }

        // Stub events
        private void CmbCourseFilterGroup_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void CmbCourseSort_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void BtnResetCourseFilter_Click(object sender, RoutedEventArgs e) { }
        private void ChkTopicAvailable_Click(object sender, RoutedEventArgs e) { }
    }
}