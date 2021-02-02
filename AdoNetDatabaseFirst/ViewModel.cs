using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AdoNetDatabaseFirst
{
    public class ViewModel : INotifyPropertyChanged
    {
        RentsEntities ctx;
        Window1 w1;
        private float minArea;
        public float MinArea
        {
            get { return minArea; }
            set
            {
                if (value != minArea)
                {
                    minArea = value;
                    OnPropertyChanged();
                }
            }
        }
        private float maxArea;
        public float MaxArea
        {
            get { return maxArea; }
            set
            {
                if (value != maxArea)
                {
                    maxArea = value;
                    OnPropertyChanged();
                }
            }
        }
        private DateTime startDate;
        public DateTime StartDate
        {
            get { return startDate; }
            set
            {
                if (value != startDate)
                {
                    startDate = value;
                    OnPropertyChanged();
                    
                }
            }
        }

        private DateTime endDate;
        public DateTime EndDate
        {
            get { return endDate; }
            set
            {
                if (value != endDate)
                {
                    endDate = value;
                    OnPropertyChanged();
                }
            }
        }

        //-------------------------------------------------------------------------------------------

        private Rent selectedCurrentRent;
        private ArchiveRent selectedPastRent;
        private Premis selectedPremises;

        public Rent SelectedCurrentRent
        {
            get
            {
                return selectedCurrentRent;
            }
            set
            {
                if (value != selectedCurrentRent)
                {
                    selectedCurrentRent = value;
                    OnPropertyChanged();
                }
            }
        }
        public ArchiveRent SelectedPastRent
        {
            get
            {
                return selectedPastRent;
            }
            set
            {
                if (value != selectedPastRent)
                {
                    selectedPastRent = value;
                    OnPropertyChanged();
                }
            }
        }
        public Premis SelectedPremises
        {
            get { return selectedPremises; }
            set
            {
                if (value != selectedPremises)
                {
                    selectedPremises = value;
                    OnPropertyChanged();
                }

            }
        }


        private User currentUser;
        public User CurrentUser
        {
            get { return currentUser; }
            set
            {
                if (value != currentUser)
                {
                    currentUser = value;
                    OnPropertyChanged();
                }
            }
        }

        private Country selectedCountry;
        public Country SelectedCountry
        {
            get { return selectedCountry; }
            set
            {
                if (value != selectedCountry)
                {
                    selectedCountry = value;
                    FillCities();
                    OnPropertyChanged();
                }
            }
        }
        private City selectedCity;
        public City SelectedCity
        {
            get { return selectedCity; }
            set
            {
                if (value != selectedCity)
                {
                    selectedCity = value;
                    OnPropertyChanged();
                }
            }
        }


        //-------------------------------------------------------------------------------------------
        private readonly ICollection<Rent> currentRents = new ObservableCollection<Rent>();
        public IEnumerable<Rent> CurrentRents => currentRents;

        private readonly ICollection<ArchiveRent> pastRents = new ObservableCollection<ArchiveRent>();
        public IEnumerable<ArchiveRent> PastRents => pastRents;

        private readonly ICollection<Premis> premises = new ObservableCollection<Premis>();
        public IEnumerable<Premis> Premises => premises;

        private readonly ICollection<User> users = new ObservableCollection<User>();
        public IEnumerable<User> Users => users;

        private readonly ICollection<Country> countries = new ObservableCollection<Country>();
        public IEnumerable<Country> Countries => countries;

        private readonly ICollection<City> cities = new ObservableCollection<City>();
        public IEnumerable<City> Cities => cities;


        //-------------------------------------------------------------------------------------------
        private Command loginCommand;
        private Command registrationCommand;
        private Command loginRegistrationShowCommand;
        private Command rentCommand;
        private Command removeCommand;
        private Command exitCommand;
        private Command searchCommand;


        public ICommand LoginCommand => loginCommand;
        public ICommand RegistrationCommand => registrationCommand;
        public ICommand LoginRegistrationShowCommand => loginRegistrationShowCommand;
        public ICommand RentCommand => rentCommand;
        public ICommand RemoveCommand => removeCommand;
        public ICommand ExitCommand => exitCommand;
        public ICommand SearchCommand => searchCommand;



        //-------------------------------------------------------------------------------------------
        public void ShowLoginRegistrationForm()
        {
            try
            {
                w1 = new Window1();

                CurrentUser = new User();
                w1.DataContext = this;

                w1.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        public void Exit()
        {
            try
            {
                currentUser = new User();
                pastRents.Clear();
                currentRents.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void Login()
        {
            try
            {
                w1.Close();

                CurrentUser.Password=  ComputeSha256Hash(currentUser.Password);
                CurrentUser = ctx.Users.FirstOrDefault(u => u.Login == currentUser.Login && CurrentUser.Password == u.Password);
                if (CurrentUser != null)
                {


                    IEnumerable<Rent> current = ctx.Rents.Where(r => r.UserId == CurrentUser.Id);
                    currentRents.Clear();
                    foreach (var item in current)
                    {
                        currentRents.Add(item);
                    }
                    IEnumerable<ArchiveRent> past = ctx.ArchiveRents.Where(r => r.UserId == CurrentUser.Id);
                    pastRents.Clear();
                    foreach (var item in past)
                    {
                        pastRents.Add(item);
                    }
                 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void FillCities()
        {
            try
            {


                cities.Clear();
                var res = ctx.Cities.Where(c => c.CountryId == selectedCountry.Id);
                foreach (var item in res)
                {
                    cities.Add(item);
                }
            }  catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void Registration()
        {
            try { 
            w1.Close();
            string sha256 = ComputeSha256Hash(currentUser.Password);
            var user = ctx.Users.FirstOrDefault(u => u.Login == currentUser.Login && sha256 == u.Password && u.Email == currentUser.Email);
                if (user == null)
                {
                    user = new User()
                    {
                        Login = currentUser.Login,
                        Email = currentUser.Email,
                        Name = currentUser.Name,
                        Password = sha256
                    };
                    ctx.Users.Add(user);
                    ctx.SaveChanges();

                    currentUser = ctx.Users.FirstOrDefault(u => u.Login == user.Login);
                    users.Add(currentUser);
                }
                }  catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        
        }
        public void AddRent()
        {
            try
            {
                if (StartDate < EndDate && StartDate>=DateTime.Now)
                {
                    Rent newRent = new Rent()
                    {
                        UserId = currentUser.Id,
                        PremisesId = selectedPremises.Id,
                        StartDate = this.StartDate,
                        EndDate = this.EndDate
                    };
                    ctx.Rents.Add(newRent);
                    ctx.SaveChanges();
                    currentRents.Add(newRent);
                    Filter();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void RemoveCurrentRent()
        {
            try
            {
                Rent rent = ctx.Rents.FirstOrDefault(r => r.Id == SelectedCurrentRent.Id);
                ctx.Rents.Remove(rent);
                currentRents.Remove(rent);
                ArchiveRent archiveRents = new ArchiveRent()
                {
                    UserId = rent.UserId,
                    PremisesId = rent.PremisesId,
                    StartDate = rent.StartDate,
                    EndDate = rent.EndDate
                };
                ctx.ArchiveRents.Add(archiveRents);
                pastRents.Add(archiveRents);
                ctx.SaveChanges();
                Filter();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void Filter()
        {
            try {
                ICollection<Premis> res = ctx.Premises.Where(p => p.Area >= minArea && p.Area <= maxArea && p.City.CountryId == selectedCountry.Id && p.CityId == selectedCity.Id && StartDate < EndDate &&
                p.Rents.Count(r => (StartDate <= r.StartDate && r.EndDate <= EndDate) || (startDate <= r.StartDate && EndDate <= r.EndDate && r.StartDate < EndDate) || (r.StartDate <= StartDate && EndDate <= r.EndDate && StartDate < EndDate) || (r.StartDate <= startDate && r.EndDate <= EndDate && StartDate < r.EndDate)) == 0).ToList();

                premises.Clear();
                selectedPremises = new Premis();
                foreach (var item in res)
                {
                    premises.Add(item);
                }
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void Search()
        {
            try
            {
                Filter();
               
               
                if (premises.Count == 0)
                    MessageBox.Show("Premises didn't founded");
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        public ViewModel()
        {
            ctx = new RentsEntities();

            loginCommand = new DelegateCommand(Login);
            registrationCommand = new DelegateCommand(Registration);
            loginRegistrationShowCommand = new DelegateCommand(ShowLoginRegistrationForm);
            rentCommand = new DelegateCommand(AddRent);                                   
            removeCommand = new DelegateCommand(RemoveCurrentRent);                       
            exitCommand = new DelegateCommand(Exit);                                      
            searchCommand = new DelegateCommand(Search);                                  
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
       
            currentUser = new User();

            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(currentUser))
                {
                    removeCommand.RaiseCanExecuteChanged();
                    exitCommand.RaiseCanExecuteChanged();
                    rentCommand.RaiseCanExecuteChanged();
                    loginCommand.RaiseCanExecuteChanged();
                    registrationCommand.RaiseCanExecuteChanged();
                    loginRegistrationShowCommand.RaiseCanExecuteChanged();
                    searchCommand.RaiseCanExecuteChanged();

                }
               
            };
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(selectedPremises))
                {
                    searchCommand.RaiseCanExecuteChanged();
                }
            };
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SelectedCountry))
                {
                    searchCommand.RaiseCanExecuteChanged();
                    rentCommand.RaiseCanExecuteChanged();
                }
            };
            var countries = ctx.Countries;
            foreach (var item in countries)
            {
                this.countries.Add(item);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        static string ComputeSha256Hash(string rawData)
        {

            using (SHA256 sha256Hash = SHA256.Create())
            {

                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

    }
}
