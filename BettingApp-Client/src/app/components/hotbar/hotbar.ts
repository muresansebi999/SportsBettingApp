import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core'; 
import { CommonModule } from '@angular/common'; 
import { FormsModule } from '@angular/forms'; 
import { HotbarService } from '../../services/hotbar';  

@Component({
  selector: 'app-hotbar',
  standalone: true,
  imports: [CommonModule, FormsModule], 
  templateUrl: './hotbar.html', 
  styleUrls: ['./hotbar.css']   
})
export class HotbarComponent implements OnInit, OnDestroy {
  username: string = '';
  balance: number = 0;
  firstName: string = '';
  lastName: string = '';
  email: string = '';
  dateOfBirth: string = '';

  editUsername: string = '';
  editEmail: string = '';

  isEditingUsername: boolean = false;
  isEditingEmail: boolean = false;

  showDepositModal: boolean = false; 
  showProfileModal: boolean = false;
  showProfileMenu: boolean = false;
  
  showToast: boolean = false;
  toastMessage: string = '';

  private checkInterval: any; 
  private lastKnownUser: string = '';

  constructor(
    private hotbarService: HotbarService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadUserInfo();

    this.checkInterval = setInterval(() => {
      const currentUser = this.hotbarService.getCurrentUser();
      const actualName = currentUser ? (currentUser.username || currentUser.Username) : '';
      
      if (actualName !== this.lastKnownUser) {
        this.lastKnownUser = actualName;
        
        if (actualName) {
          this.loadUserInfo(); 
        } else {
          this.username = 'U';
          this.balance = 0;
          this.firstName = '';
          this.lastName = '';
          this.email = '';
          this.dateOfBirth = '';
        }
        this.cdr.detectChanges();
      }
    }, 500);
  }

  ngOnDestroy(): void {
    if (this.checkInterval) {
      clearInterval(this.checkInterval);
    }
  }

  loadUserInfo() {
    this.hotbarService.getHotbarInfo().subscribe({
      next: (data: any) => {
        this.username = data.username || data.Username || 'U';
        this.balance = data.balance || data.Balance || 0;
        this.firstName = data.firstName || data.FirstName || '-';
        this.lastName = data.lastName || data.LastName || '-';
        this.email = data.email || data.Email || '-';
        this.dateOfBirth = data.dateOfBirth || data.DateOfBirth || 'Nespecificată';

        this.editUsername = this.username;
        this.editEmail = this.email;
        this.cdr.detectChanges();
      },
      error: (err: any) => console.error('Eroare la încărcarea datelor:', err)
    });
  }

  openModal() {
    this.showDepositModal = true;
    this.showProfileModal = false;
    this.showProfileMenu = false; 
  }
  closeModal() { this.showDepositModal = false; }

  openProfileModal() {
    this.showProfileModal = true;
    this.showDepositModal = false;
    this.showProfileMenu = false; 
    
    this.isEditingUsername = false;
    this.isEditingEmail = false;
    this.editUsername = this.username;
    this.editEmail = this.email;
  }
  closeProfileModal() { this.showProfileModal = false; }

  toggleProfileMenu() { this.showProfileMenu = !this.showProfileMenu; }

  onLogout() {
    this.showProfileMenu = false; 
    this.hotbarService.logout();  
    window.location.reload();
  }

  saveProfileChanges() {
    if(!this.isEditingUsername && !this.isEditingEmail) {
      this.closeProfileModal();
      return;
    }

    if(!this.editUsername || !this.editEmail) {
      alert("Câmpurile nu pot fi goale!");
      return;
    }

    this.hotbarService.updateProfile(this.username, this.editUsername, this.editEmail).subscribe({
      next: (response: any) => {
        const userStr = localStorage.getItem('user');
        if (userStr) {
          let userObj = JSON.parse(userStr);
          userObj.username = response.newUsername;
          userObj.email = response.newEmail;
          localStorage.setItem('user', JSON.stringify(userObj));
        }

        this.username = response.newUsername;
        this.email = response.newEmail;
        this.lastKnownUser = response.newUsername; 

        this.closeProfileModal();
        
        setTimeout(() => {
          this.displaySuccessToast(`Profilul a fost actualizat cu succes!`);
        }, 150);
      },
      error: (err: any) => {
        console.error("Eroare Detaliată:", err); // Afisam si in consola F12

        // Extragem eroare EXACTĂ pentru a vedea de ce crapă
        let errorMsg = 'Eroare de rețea. Te rog verifică dacă C# (backend-ul) rulează.';
        
        if (err.status === 409) {
          errorMsg = 'Acest username este deja luat!';
        } else if (err.error && err.error.message) {
          errorMsg = err.error.message;
        } else if (err.error && typeof err.error === 'string') {
          errorMsg = err.error;
        } else if (err.message) {
          errorMsg = err.message;
        }

        alert(`Eroare:\n${errorMsg}`);
      }
    });
  }

  displaySuccessToast(message: string) {
    this.toastMessage = message;
    this.showToast = true;
    this.cdr.detectChanges(); 
    setTimeout(() => {
      this.showToast = false;
      this.cdr.detectChanges(); 
    }, 2000); 
  }

  confirmDeposit(amount: number) {
    this.hotbarService.depositFunds(amount).subscribe({
      next: (response: any) => {
        const newBal = response.newBalance || response.NewBalance;
        this.balance = newBal;
        
        const userStr = localStorage.getItem('user');
        if (userStr) {
          let userObj = JSON.parse(userStr);
          userObj.balance = newBal;
          localStorage.setItem('user', JSON.stringify(userObj));
        }
        this.closeModal(); 
        setTimeout(() => {
          this.displaySuccessToast(`Depunere de $${amount} realizată cu succes!`);
        }, 150);
      },
      error: (err: any) => alert('A apărut o eroare la depunere.')
    });
  }
}