using Microsoft.Win32;
using Practice.Data.Entities;
using Practice.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Practice.Windows
{
    public class StudentImportModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string RecordBook { get; set; }
    }

    public class SimpleItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } 
    }

    public partial class AdminWindow : Window
    {
        private readonly User _currentUser;
        private readonly IAdminService _adminService;
        private readonly IIdentityService _identityService;
        private readonly IPracticeService _practiceService;
        private readonly ICourseService _courseService;
        private readonly IAuditService _auditService;
        private readonly IReportingService _reportingService;

        private List<AuditLog> _allLogs = new List<AuditLog>();
        private int _editingId;
        private object _editingItem; 

        private List<Course> _cachedCourses = new List<Course>();
        private string _currentSimpleMode = "Position"; 

        public AdminWindow(
            User user,
            IAdminService adminService,
            IIdentityService identityService,
            IPracticeService practiceService,
            ICourseService courseService,
            IAuditService auditService,
            IReportingService reportingService)
        {
            InitializeComponent();
            _currentUser = user;
            _adminService = adminService;
            _identityService = identityService;
            _practiceService = practiceService;
            _courseService = courseService;
            _auditService = auditService;
            _reportingService = reportingService;

            if (TxtAdminName != null)
                TxtAdminName.Text = $"{_currentUser.LastName} {_currentUser.FirstName}";
            if (TxtInitials != null)
                TxtInitials.Text = (!string.IsNullOrEmpty(_currentUser.FirstName) ? _currentUser.FirstName[0].ToString() : "A");

            LoadAllData();
            this._reportingService = reportingService;
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
            catch (Exception ex)
            {
                MessageBox.Show($"Init Error: {ex.Message}");
            }
        }

        private async void ReloadAllDictionaries()
        {
            try
            {
                var groups = await _adminService.GetAllGroupsAsync();
                var depts = await _adminService.GetAllDepartmentsAsync();
                var specs = await _adminService.GetAllSpecialtiesAsync();
                var positions = await _adminService.GetAllPositionsAsync();
                var disciplines = await _courseService.GetAllDisciplinesAsync();
                var orgs = await _practiceService.GetAllOrganizationsAsync();

                // Отримуємо список активних курсів (для звітів та фільтрів)
                var activeCourses = await _courseService.GetAllActiveCoursesAsync();

                CmbStudGroup.ItemsSource = groups;
                CmbImportGroup.ItemsSource = groups;
                CmbFilterGroup.ItemsSource = groups;
                EditStudGroup.ItemsSource = groups;
                CmbCourseFilterGroup.ItemsSource = groups;
                CmbEnrollGroup.ItemsSource = groups;

                CmbReportGroup.ItemsSource = groups;
                CmbReportCourse.ItemsSource = activeCourses;

                CmbSupDept.ItemsSource = depts;
                CmbFilterDept.ItemsSource = depts;
                EditSupDept.ItemsSource = depts;
                CmbDeptForGroup.ItemsSource = depts;
                CmbDeptForSpec.ItemsSource = depts;
                EditStructDept.ItemsSource = depts;
                CmbOrgDeptSource.ItemsSource = depts;
                CmbReportDept.ItemsSource = depts;

                CmbSupPos.ItemsSource = positions;
                EditSupPos.ItemsSource = positions;

                CmbCourseDiscipline.ItemsSource = disciplines;
                CmbTopicDiscipline.ItemsSource = disciplines;
                EditCourseDiscipline.ItemsSource = disciplines;

                CmbSpecForGroup.ItemsSource = specs;
                EditStructSpec.ItemsSource = specs;

                CmbOrganizations.ItemsSource = orgs;
                CmbFilterTopicOrg.ItemsSource = orgs;
                EditTopicOrg.ItemsSource = orgs;

                ReloadCourseSupervisors();

                CmbReportCourse.ItemsSource = activeCourses;

                GridSpecialties.ItemsSource = specs;
                GridGroups.ItemsSource = groups;
                GridDepartments.ItemsSource = depts;

                UpdateSimpleGrid(disciplines, positions);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dict Error: " + ex.Message);
            }
        }

        private void UpdateSimpleGrid(IEnumerable<Discipline> discs, IEnumerable<Position> poses)
        {
            var list = new List<SimpleItemViewModel>();
            if (_currentSimpleMode == "Discipline")
            {
                if (TxtStructTitle != null) TxtStructTitle.Text = "Дисципліни";
                list.AddRange(discs.Select(x => new SimpleItemViewModel { Id = x.DisciplineId, Name = x.DisciplineName, Type = "Discipline" }));
            }
            else
            {
                _currentSimpleMode = "Position";
                if (TxtStructTitle != null) TxtStructTitle.Text = "Посади";
                list.AddRange(poses.Select(x => new SimpleItemViewModel { Id = x.PositionId, Name = x.PositionName, Type = "Position" }));
            }
            GridSimple.ItemsSource = list;
        }

        private async void ReloadCourseSupervisors()
        {
            var sups = await _adminService.GetSupervisorsByDeptAsync(null);
            CmbCourseSupervisor.ItemsSource = EditCourseSupervisor.ItemsSource = sups;
        }

        private async void ReloadActiveCourses() =>
            CmbEnrollCourse.ItemsSource = await _courseService.GetAllActiveCoursesAsync();

        private bool IsValidEmail(string email) =>
            !string.IsNullOrWhiteSpace(email) && email.Contains("@");

        private async Task TryDelete(Func<Task> deleteAction, string errorMessage = "Неможливо видалити запис.")
        {
            try
            {
                await deleteAction();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{errorMessage}\n{ex.Message}");
            }
        }

        private async void RefreshStudentList(int? groupId = null) =>
            GridAllStudents.ItemsSource = new ObservableCollection<Student>(await _adminService.GetStudentsByGroupAsync(groupId));

        private void CmbFilterGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbFilterGroup.SelectedValue is int gid) RefreshStudentList(gid);
        }

        private void BtnResetStudentFilter_Click(object sender, RoutedEventArgs e)
        {
            CmbFilterGroup.SelectedIndex = -1;
            RefreshStudentList();
        }

        private async void BtnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbStudGroup.SelectedValue == null) throw new Exception("Оберіть групу");
                var u = new User { LastName = TxtStudLast.Text.Trim(), FirstName = TxtStudFirst.Text.Trim(), Email = TxtStudEmail.Text.Trim(), RoleId = 2 };
                await _identityService.RegisterStudentAsync(u, TxtStudPass.Text, (int)CmbStudGroup.SelectedValue, TxtStudRecord.Text);

                MessageBox.Show("Студента додано!");
                TxtStudLast.Clear(); TxtStudFirst.Clear(); TxtStudEmail.Clear(); TxtStudPass.Clear(); TxtStudRecord.Clear();
                RefreshStudentList((int?)CmbFilterGroup.SelectedValue);
                LoadLogs();
            }
            catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
        }

        private async void BtnDeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int userId && MessageBox.Show("Видалити?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _adminService.DeleteStudentAsync(userId);
                RefreshStudentList((int?)CmbFilterGroup.SelectedValue);
                LoadLogs();
            }
        }

        private async void BtnEditStudent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int studentId)
            {
                var s = await _adminService.GetStudentByIdAsync(studentId);
                if (s != null)
                {
                    _editingId = studentId;
                    EditStudLast.Text = s.User.LastName;
                    EditStudFirst.Text = s.User.FirstName;
                    EditStudEmail.Text = s.User.Email;
                    EditStudRecord.Text = s.RecordBookNumber;
                    EditStudGroup.SelectedValue = s.GroupId;
                    ModalEditStudent.Visibility = Visibility.Visible;
                }
            }
        }

        private async void BtnSaveEditStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EditStudGroup.SelectedValue == null) return;
                await _adminService.UpdateStudentAsync(_editingId, EditStudFirst.Text, EditStudLast.Text, EditStudEmail.Text, EditStudRecord.Text, (int)EditStudGroup.SelectedValue);
                RefreshStudentList((int?)CmbFilterGroup.SelectedValue);
                ModalEditStudent.Visibility = Visibility.Collapsed;
                LoadLogs();
            }
            catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
        }

        private void BtnCancelEditStudent_Click(object sender, RoutedEventArgs e) =>
            ModalEditStudent.Visibility = Visibility.Collapsed;

        private void BtnToggleImport_Click(object sender, RoutedEventArgs e) =>
            PanelImport.Visibility = (PanelImport.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;

        private void BtnCancelImport_Click(object sender, RoutedEventArgs e) =>
            PanelImport.Visibility = Visibility.Collapsed;

        private void BtnProcessImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbImportGroup.SelectedValue == null)
                {
                    MessageBox.Show("Спочатку оберіть групу!");
                    return;
                }

                var lines = TxtRawNames.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var previewList = new List<StudentImportModel>();
                string domain = TxtImportDomain.Text.Trim(); // наприклад @student.university.edu
                int counter = 100; // Для генерації унікальних номерів (демо)

                foreach (var line in lines)
                {
                    var parts = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) continue;

                    string last = parts[0];
                    string first = parts[1];

                    string emailLogin = $"{Transliterate(first)}.{Transliterate(last)}".ToLower();
                    string email = $"{emailLogin}{domain}";

                    previewList.Add(new StudentImportModel
                    {
                        LastName = last,
                        FirstName = first,
                        Email = email,
                        RecordBook = $"RB-{DateTime.Now.Year}-{counter++}"
                    });
                }

                GridImportPreview.ItemsSource = previewList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка обробки: " + ex.Message);
            }
        }

        private async void BtnSaveImport_Click(object sender, RoutedEventArgs e)
        {
            var list = GridImportPreview.ItemsSource as List<StudentImportModel>;
            if (list == null || list.Count == 0) return;

            if (CmbImportGroup.SelectedValue == null)
            {
                MessageBox.Show("Оберіть групу для імпорту!");
                return;
            }

            int groupId = (int)CmbImportGroup.SelectedValue;
            int successCount = 0;

            foreach (var item in list)
            {
                try
                {
                    var newUser = new User
                    {
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Email = item.Email,
                        RoleId = 2 // Student
                    };

                    await _identityService.RegisterStudentAsync(newUser, "123456", groupId, item.RecordBook);
                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка імпорту {item.Email}: {ex.Message}");
                }
            }

            MessageBox.Show($"Імпортовано: {successCount} з {list.Count}");

            PanelImport.Visibility = Visibility.Collapsed;
            GridImportPreview.ItemsSource = null;
            TxtRawNames.Clear();
            CmbFilterGroup.SelectedValue = groupId;
            RefreshStudentList(groupId);
            LoadLogs();
        }

        private string Transliterate(string ukrText)
        {
            string[] ukr = { "а", "б", "в", "г", "ґ", "д", "е", "є", "ж", "з", "и", "і", "ї", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ь", "ю", "я", "А", "Б", "В", "Г", "Ґ", "Д", "Е", "Є", "Ж", "З", "И", "І", "Ї", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ь", "Ю", "Я" };
            string[] eng = { "a", "b", "v", "h", "g", "d", "e", "ye", "zh", "z", "y", "i", "yi", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "kh", "ts", "ch", "sh", "shch", "", "yu", "ya", "A", "B", "V", "H", "G", "D", "E", "Ye", "Zh", "Z", "Y", "I", "Yi", "Y", "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F", "Kh", "Ts", "Ch", "Sh", "Shch", "", "Yu", "Ya" };

            for (int i = 0; i < ukr.Length; i++)
            {
                ukrText = ukrText.Replace(ukr[i], eng[i]);
            }
            return ukrText;
        }

        private async void RefreshSupervisorList(int? deptId = null) =>
            GridAllSupervisors.ItemsSource = new ObservableCollection<Supervisor>(await _adminService.GetSupervisorsByDeptAsync(deptId));

        private void CmbFilterDept_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbFilterDept.SelectedValue is int did) RefreshSupervisorList(did);
        }

        private void BtnResetSupervisorFilter_Click(object sender, RoutedEventArgs e)
        {
            CmbFilterDept.SelectedIndex = -1;
            RefreshSupervisorList();
        }

        private async void BtnAddSupervisor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbSupDept.SelectedValue == null)
                {
                    MessageBox.Show("Оберіть кафедру! Це обов'язково.");
                    return;
                }

                int deptId = (int)CmbSupDept.SelectedValue;
                int? posId = CmbSupPos.SelectedValue as int?;

                var u = new User
                {
                    LastName = TxtSupLast.Text,
                    FirstName = TxtSupFirst.Text,
                    Email = TxtSupEmail.Text,
                    RoleId = 3 
                };

                await _identityService.RegisterSupervisorAsync(u, TxtSupPass.Text, deptId, posId, TxtSupPhone.Text);

                MessageBox.Show("Керівника додано!");

                RefreshSupervisorList((int?)CmbFilterDept.SelectedValue);
                ReloadCourseSupervisors(); 

                LoadLogs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка додавання: " + ex.Message + "\n" + ex.InnerException?.Message);
            }
        }

        private async void BtnDeleteSupervisor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int userId && MessageBox.Show("Видалити?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _adminService.DeleteSupervisorAsync(userId);
                RefreshSupervisorList((int?)CmbFilterDept.SelectedValue);
                LoadLogs();
            }
        }

        private async void BtnEditSupervisor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int supId)
            {
                var s = await _adminService.GetSupervisorByIdAsync(supId);
                if (s != null)
                {
                    _editingId = supId;
                    EditSupLast.Text = s.User.LastName;
                    EditSupFirst.Text = s.User.FirstName;
                    EditSupEmail.Text = s.User.Email;
                    EditSupPhone.Text = s.Phone;
                    EditSupDept.SelectedValue = s.DepartmentId;
                    EditSupPos.SelectedValue = s.PositionId;
                    ModalEditSupervisor.Visibility = Visibility.Visible;
                }
            }
        }

        private async void BtnSaveEditSupervisor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _adminService.UpdateSupervisorAsync(_editingId, EditSupFirst.Text, EditSupLast.Text, EditSupEmail.Text, EditSupPhone.Text, (int)EditSupDept.SelectedValue, (int?)EditSupPos.SelectedValue);
                RefreshSupervisorList((int?)CmbFilterDept.SelectedValue);
                ModalEditSupervisor.Visibility = Visibility.Collapsed;
                LoadLogs();
            }
            catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
        }

        private void StructMenu_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelAddSpecialty == null) return;
            PanelAddSpecialty.Visibility = PanelAddGroup.Visibility = PanelAddDept.Visibility = PanelAddSimple.Visibility = Visibility.Collapsed;
            GridSpecialties.Visibility = GridGroups.Visibility = GridDepartments.Visibility = GridSimple.Visibility = Visibility.Collapsed;

            if (RbSpecialties.IsChecked == true)
            {
                TxtStructTitle.Text = "Спеціальності";
                PanelAddSpecialty.Visibility = Visibility.Visible;
                GridSpecialties.Visibility = Visibility.Visible;
            }
            else if (RbGroups.IsChecked == true)
            {
                TxtStructTitle.Text = "Групи";
                PanelAddGroup.Visibility = Visibility.Visible;
                GridGroups.Visibility = Visibility.Visible;
            }
            else if (RbDepartments.IsChecked == true)
            {
                TxtStructTitle.Text = "Кафедри";
                PanelAddDept.Visibility = Visibility.Visible;
                GridDepartments.Visibility = Visibility.Visible;
            }
            else
            {
                bool isDisc = RbDisciplines.IsChecked == true;
                _currentSimpleMode = isDisc ? "Discipline" : "Position";
                TxtStructTitle.Text = isDisc ? "Дисципліни" : "Посади";
                PanelAddSimple.Visibility = Visibility.Visible;
                GridSimple.Visibility = Visibility.Visible;
                ReloadAllDictionaries();
            }
        }

        private async void BtnAddSpecialty_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbDeptForSpec.SelectedValue == null) return;
                await _adminService.AddSpecialtyAsync(new Specialty { Code = TxtSpecCode.Text, Name = TxtSpecName.Text, DepartmentId = (int)CmbDeptForSpec.SelectedValue });
                TxtSpecName.Clear(); TxtSpecCode.Clear();
                ReloadAllDictionaries();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void BtnAddGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbSpecForGroup.SelectedValue == null) return;
                await _adminService.AddGroupAsync(new StudentGroup { GroupCode = TxtGroupCode.Text, SpecialtyId = (int)CmbSpecForGroup.SelectedValue, EntryYear = int.Parse(TxtGroupYear.Text) });
                ReloadAllDictionaries();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void CmbDeptForGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbDeptForGroup.SelectedValue is int deptId)
            {
                var allSpecs = await _adminService.GetAllSpecialtiesAsync();
                CmbSpecForGroup.ItemsSource = allSpecs.Where(s => s.DepartmentId == deptId).ToList();
                if (CmbSpecForGroup.Items.Count > 0) CmbSpecForGroup.SelectedIndex = 0;
            }
        }

        private async void BtnAddDept_Click(object sender, RoutedEventArgs e)
        {
            await _adminService.AddDepartmentAsync(new Department { DepartmentName = TxtDeptName.Text });
            TxtDeptName.Clear();
            ReloadAllDictionaries();
        }

        private async void BtnAddSimple_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RbDisciplines.IsChecked == true) await _courseService.AddDisciplineAsync(TxtSimpleName.Text);
                else await _adminService.AddPositionAsync(new Position { PositionName = TxtSimpleName.Text });
                TxtSimpleName.Clear();
                ReloadAllDictionaries();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void BtnDeleteStruct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && MessageBox.Show("Видалити?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (btn.Tag is Specialty s) await _adminService.DeleteSpecialtyAsync(s.SpecialtyId);
                else if (btn.Tag is StudentGroup g) await _adminService.DeleteGroupAsync(g.GroupId);
                else if (btn.Tag is Department d) await _adminService.DeleteDepartmentAsync(d.DepartmentId);
                else if (btn.Tag is SimpleItemViewModel sim)
                {
                    if (sim.Type == "Discipline") await _adminService.DeleteDisciplineAsync(sim.Id);
                    else await _adminService.DeletePositionAsync(sim.Id);
                }
                ReloadAllDictionaries();
            }
        }

        private void BtnEditStruct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                _editingItem = btn.Tag;
                ModalEditStruct.Visibility = Visibility.Visible;

                PanelEditStructCode.Visibility = Visibility.Collapsed;
                PanelEditStructYear.Visibility = Visibility.Collapsed;
                PanelEditStructDept.Visibility = Visibility.Collapsed;
                PanelEditStructSpec.Visibility = Visibility.Collapsed;
                PanelEditStructName.Visibility = Visibility.Visible;

                if (_editingItem is Specialty s)
                {
                    LblStructName.Text = "Назва спеціальності:";
                    EditStructName.Text = s.Name;

                    PanelEditStructCode.Visibility = Visibility.Visible;
                    LblStructCode.Text = "Код:";
                    EditStructCode.Text = s.Code;

                    PanelEditStructDept.Visibility = Visibility.Visible;
                    EditStructDept.SelectedValue = s.DepartmentId;
                }
                else if (_editingItem is StudentGroup g)
                {
                    LblStructName.Text = "Шифр групи:";
                    EditStructName.Text = g.GroupCode;

                    PanelEditStructYear.Visibility = Visibility.Visible;
                    EditStructYear.Text = g.EntryYear.ToString();

                    PanelEditStructSpec.Visibility = Visibility.Visible;
                    EditStructSpec.SelectedValue = g.SpecialtyId;
                }
                else if (_editingItem is Department d)
                {
                    LblStructName.Text = "Назва кафедри:";
                    EditStructName.Text = d.DepartmentName;
                }
                else if (_editingItem is SimpleItemViewModel sim)
                {
                    LblStructName.Text = "Назва:";
                    EditStructName.Text = sim.Name;
                }
            }
        }

        private async void BtnSaveEditStruct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_editingItem is Specialty s)
                {
                    s.Name = EditStructName.Text;
                    s.Code = EditStructCode.Text;
                    if (EditStructDept.SelectedValue is int deptId) s.DepartmentId = deptId;
                    await _adminService.UpdateSpecialtyAsync(s);
                }
                else if (_editingItem is StudentGroup g)
                {
                    g.GroupCode = EditStructName.Text;
                    if (int.TryParse(EditStructYear.Text, out int y)) g.EntryYear = y;
                    if (EditStructSpec.SelectedValue is int specId) g.SpecialtyId = specId;
                    await _adminService.UpdateGroupAsync(g);
                }
                else if (_editingItem is Department d)
                {
                    d.DepartmentName = EditStructName.Text;
                    await _adminService.UpdateDepartmentAsync(d);
                }
                else if (_editingItem is SimpleItemViewModel sim)
                {
                    if (sim.Type == "Position")
                    {
                        await _adminService.UpdatePositionAsync(new Position { PositionId = sim.Id, PositionName = EditStructName.Text });
                    }
                    else if (sim.Type == "Discipline")
                    {
                        await _adminService.UpdateDisciplineAsync(new Discipline { DisciplineId = sim.Id, DisciplineName = EditStructName.Text });
                    }
                }

                ModalEditStruct.Visibility = Visibility.Collapsed;
                ReloadAllDictionaries();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження: {ex.Message}");
            }
        }

        private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
        {
            ModalEditStudent.Visibility = ModalEditOrg.Visibility = ModalEditSupervisor.Visibility =
            ModalEditStruct.Visibility = ModalEditTopic.Visibility = ModalEditCourse.Visibility = Visibility.Collapsed;
        }

        private void EditStructDept_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private async void RefreshPracticeTab(int? orgId = null, string type = null)
        {
            var topics = await _practiceService.GetAvailableTopicsAsync();
            if (orgId.HasValue) topics = topics.Where(t => t.OrganizationId == orgId.Value);
            GridTopics.ItemsSource = new ObservableCollection<InternshipTopic>(topics);

            var orgs = await _practiceService.GetAllOrganizationsAsync();
            if (!string.IsNullOrEmpty(type))
            {
                string k = type == "Зовнішня" ? "External" : "University";
                orgs = orgs.Where(o => o.Type == k);
            }
            GridOrgs.ItemsSource = new ObservableCollection<Organization>(orgs);
        }

        private async void BtnAddOrg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name;
                string type;

                if (CmbOrgType.SelectedIndex == 1) // Кафедра (University)
                {
                    if (CmbOrgDeptSource.SelectedValue == null) throw new Exception("Оберіть кафедру зі списку!");
                    name = CmbOrgDeptSource.SelectedValue.ToString() + " (Внутрішня)";
                    type = "University";
                }
                else // Зовнішня
                {
                    if (string.IsNullOrWhiteSpace(TxtOrgName.Text)) throw new Exception("Введіть назву організації!");
                    name = TxtOrgName.Text;
                    type = "External";
                }

                await _practiceService.CreateOrganizationAsync(name, TxtOrgAddr.Text, type, TxtOrgEmail.Text);

                RefreshPracticeTab();
                ReloadAllDictionaries(); // Оновлює випадайки в темах
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void BtnAddTopic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _practiceService.AddTopicAsync(new InternshipTopic { Title = TxtTopicTitle.Text, Description = TxtTopicDesc.Text, OrganizationId = (int)CmbOrganizations.SelectedValue, DisciplineId = (int)CmbTopicDiscipline.SelectedValue });
                RefreshPracticeTab();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void CmbFilterTopicOrg_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            RefreshPracticeTab((int?)CmbFilterTopicOrg.SelectedValue, (CmbFilterOrgType.SelectedItem as ComboBoxItem)?.Content.ToString());

        private void CmbFilterOrgType_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            RefreshPracticeTab((int?)CmbFilterTopicOrg.SelectedValue, (CmbFilterOrgType.SelectedItem as ComboBoxItem)?.Content.ToString());

        private void BtnResetTopicFilter_Click(object sender, RoutedEventArgs e)
        {
            CmbFilterTopicOrg.SelectedIndex = -1;
            RefreshPracticeTab();
        }

        private void BtnResetOrgFilter_Click(object sender, RoutedEventArgs e)
        {
            CmbFilterOrgType.SelectedIndex = -1;
            RefreshPracticeTab();
        }

        private async void BtnDeleteTopic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is int id) { await _practiceService.DeleteTopicAsync(id); RefreshPracticeTab(); }
        }

        private async void BtnDeleteOrg_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is int id) { await _practiceService.DeleteOrganizationAsync(id); RefreshPracticeTab(); ReloadAllDictionaries(); }
        }

        private async void ChkTopicAvailable_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk && chk.DataContext is InternshipTopic t)
            {
                t.IsAvailable = chk.IsChecked == true;
                await _practiceService.UpdateTopicAsync(t);
            }
        }

        private void BtnEditOrg_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is int id)
            {
                var o = (GridOrgs.ItemsSource as IEnumerable<Organization>).FirstOrDefault(x => x.OrganizationId == id);
                if (o != null)
                {
                    _editingId = id;
                    EditOrgName.Text = o.Name;
                    EditOrgAddr.Text = o.Address;
                    EditOrgEmail.Text = o.ContactEmail;
                    ModalEditOrg.Visibility = Visibility.Visible;
                }
            }
        }

        private async void BtnSaveEditOrg_Click(object sender, RoutedEventArgs e)
        {
            await _adminService.UpdateOrganizationAsync(new Organization { OrganizationId = _editingId, Name = EditOrgName.Text, Address = EditOrgAddr.Text, Type = EditOrgType.SelectedIndex == 0 ? "External" : "University", ContactEmail = EditOrgEmail.Text });
            ModalEditOrg.Visibility = Visibility.Collapsed;
            RefreshPracticeTab();
        }

        private void BtnCancelEditOrg_Click(object sender, RoutedEventArgs e) => ModalEditOrg.Visibility = Visibility.Collapsed;

        private void BtnEditTopic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is int id)
            {
                var t = (GridTopics.ItemsSource as IEnumerable<InternshipTopic>).FirstOrDefault(x => x.TopicId == id);
                if (t != null)
                {
                    _editingId = id;
                    EditTopicTitle.Text = t.Title;
                    EditTopicDesc.Text = t.Description;
                    EditTopicOrg.SelectedValue = t.OrganizationId;
                    ModalEditTopic.Visibility = Visibility.Visible;
                }
            }
        }

        private async void BtnSaveEditTopic_Click(object sender, RoutedEventArgs e)
        {
            await _practiceService.UpdateTopicAsync(new InternshipTopic { TopicId = _editingId, Title = EditTopicTitle.Text, Description = EditTopicDesc.Text, OrganizationId = (int)EditTopicOrg.SelectedValue });
            ModalEditTopic.Visibility = Visibility.Collapsed;
            RefreshPracticeTab();
        }

        private void CmbOrgType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelOrgNameInput == null) return;
            bool isUni = CmbOrgType.SelectedIndex == 1;
            PanelOrgDeptSelect.Visibility = isUni ? Visibility.Visible : Visibility.Collapsed;
            PanelOrgNameInput.Visibility = isUni ? Visibility.Collapsed : Visibility.Visible;
        }
        private async void RefreshCoursesList()
        {
            var courses = await _courseService.GetAllActiveCoursesAsync();
            _cachedCourses = courses.ToList();
            ApplyCourseFilters();
            CmbEnrollCourse.ItemsSource = null;
            CmbEnrollCourse.ItemsSource = _cachedCourses;
        }

        private void ApplyCourseFilters()
        {
            if (_cachedCourses == null) return;
            var q = _cachedCourses.AsEnumerable();

            if (CmbCourseFilterGroup.SelectedValue is int gid)
                q = q.Where(c => c.CourseEnrollments != null && c.CourseEnrollments.Any(e => e.GroupId == gid));
            if (CmbCourseSort.SelectedIndex == 0) q = q.OrderBy(c => c.Name);
            else if (CmbCourseSort.SelectedIndex == 1) q = q.OrderBy(c => c.Discipline?.DisciplineName);
            GridCourses.ItemsSource = new ObservableCollection<Course>(q);
        }

        private void CmbCourseFilterGroup_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyCourseFilters();
        private void CmbCourseSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyCourseFilters();
        private void BtnResetCourseFilter_Click(object sender, RoutedEventArgs e) { CmbCourseFilterGroup.SelectedIndex = -1; ApplyCourseFilters(); }

        private async void BtnAddCourse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbCourseDiscipline.SelectedValue == null) throw new Exception("Оберіть дисципліну!");
                var c = new Course
                {
                    Name = TxtCourseName.Text,
                    Year = int.Parse(TxtCourseYear.Text),
                    DisciplineId = (int)CmbCourseDiscipline.SelectedValue,
                    IsActive = true,
                    SupervisorId = CmbCourseSupervisor.SelectedValue as int?
                };

                await _courseService.CreateCourseAsync(c);
                MessageBox.Show("Курс створено!");
                RefreshCoursesList();
                TxtCourseName.Clear();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void BtnEnrollGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbEnrollCourse.SelectedValue == null || CmbEnrollGroup.SelectedValue == null) throw new Exception("Оберіть курс та групу!");
                int courseId = (int)CmbEnrollCourse.SelectedValue;
                int groupId = (int)CmbEnrollGroup.SelectedValue;

                var students = await _adminService.GetStudentsByGroupAsync(groupId);
                foreach (var s in students) await _courseService.EnrollStudentToCourseAsync(s.StudentId, courseId, groupId);

                MessageBox.Show($"Групу зараховано!");
                RefreshCoursesList();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void BtnUnenrollOneGroup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CourseEnrollment enr)
            {
                if (MessageBox.Show($"Відрахувати групу {enr.StudentGroup?.GroupCode}?", "Підтвердження", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _courseService.UnenrollGroupFromCourseAsync(enr.CourseId, enr.GroupId ?? 0);
                        RefreshCoursesList();
                    }
                    catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
                }
            }
        }

        private async void BtnDeleteCourse_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var courseToDelete = _cachedCourses.FirstOrDefault(c => c.CourseId == id);
                string courseName = courseToDelete?.Name ?? "Unknown";

                if (MessageBox.Show($"Видалити курс '{courseName}'?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    await _courseService.DeleteCourseAsync(id);

                    await _auditService.LogActionAsync(_currentUser.UserId, "Delete", $"Видалено курс: {courseName}", "Course", id);

                    RefreshCoursesList();
                    LoadLogs();
                }
            }
        }

        private async void BtnSaveEditCourse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var c = new Course { CourseId = _editingId, Name = EditCourseName.Text, Year = int.Parse(EditCourseYear.Text), DisciplineId = (int)EditCourseDiscipline.SelectedValue, SupervisorId = EditCourseSupervisor.SelectedValue as int?, IsActive = true };
                await _courseService.UpdateCourseAsync(c);
                ModalEditCourse.Visibility = Visibility.Collapsed;
                RefreshCoursesList();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnEditCourse_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is int id)
            {
                var c = _cachedCourses.FirstOrDefault(x => x.CourseId == id);
                if (c != null)
                {
                    _editingId = id;
                    EditCourseName.Text = c.Name;
                    EditCourseYear.Text = c.Year.ToString();
                    EditCourseDiscipline.SelectedValue = c.DisciplineId;
                    EditCourseSupervisor.SelectedValue = c.SupervisorId;
                    ModalEditCourse.Visibility = Visibility.Visible;
                }
            }
        }

        private async void LoadLogs() { _allLogs = await _auditService.GetAllLogsAsync(); ApplyLogFilter(); }
        private void BtnRefreshLogs_Click(object sender, RoutedEventArgs e) => LoadLogs();
        private void CmbLogFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyLogFilter();

        private void ApplyLogFilter()
        {
            if (GridLogs == null) return;
            string tag = (CmbLogFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            GridLogs.ItemsSource = _allLogs.Where(l => tag == "All" || l.Action.Contains(tag)).ToList();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }
        private async void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"practice_backup_{DateTime.Now:yyyyMMdd}",
                DefaultExt = ".db",
                Filter = "SQLite Database (.db)|*.db"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _reportingService.CreateDatabaseBackup(dlg.FileName);
                    MessageBox.Show("Резервна копія створена успішно!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
            }
        }

        private async void BtnPdfReport_Click(object sender, RoutedEventArgs e)
        {
            if (CmbReportCourse.SelectedValue is int cid && CmbReportGroup.SelectedValue is int gid)
            {
                var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "PDF Files (.pdf)|*.pdf", FileName = "Vidomist_Praktiki" };
                if (dlg.ShowDialog() == true)
                {
                    await _reportingService.GeneratePdfStatementAsync(cid, gid, dlg.FileName);
                    MessageBox.Show("PDF звіт сформовано!");
                }
            }
            else { MessageBox.Show("Оберіть курс та групу!"); }
        }

        private async void BtnExcelReport_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "Excel Files (.xlsx)|*.xlsx", FileName = "Reestr_Statusiv" };
            if (dlg.ShowDialog() == true)
            {
                await _reportingService.GenerateExcelStatusReportAsync(dlg.FileName);
                MessageBox.Show("Excel звіт збережено!");
            }
        }
        private async void CmbReportDept_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbReportDept.SelectedValue is int deptId)
            {
                CmbReportCourse.ItemsSource = null;
                CmbReportGroup.ItemsSource = null;

                var allCourses = await _courseService.GetAllActiveCoursesAsync();

                var filteredCourses = allCourses
                    .Where(c => c.Supervisor != null && c.Supervisor.DepartmentId == deptId)
                    .ToList();

                CmbReportCourse.ItemsSource = filteredCourses;
            }
        }
        private async void CmbReportCourse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbReportCourse.SelectedValue is int courseId)
            {
                CmbReportGroup.ItemsSource = null;

                var enrollments = await _courseService.GetByCourseIdAsync(courseId);

                var uniqueGroups = enrollments
                    .Where(enr => enr.Student != null && enr.Student.StudentGroup != null)
                    .Select(enr => enr.Student.StudentGroup)
                    .GroupBy(g => g.GroupId)
                    .Select(g => g.First())
                    .ToList();

                CmbReportGroup.ItemsSource = uniqueGroups;
            }
        }
    }
}