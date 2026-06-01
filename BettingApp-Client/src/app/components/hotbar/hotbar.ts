import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core'; 
import { CommonModule } from '@angular/common'; 
import { FormsModule } from '@angular/forms'; 
import { HotbarService } from '../../services/hotbar';  
import { MatchService } from '../../services/match_service';

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

  showTransactionModal: boolean = false;
  transactionType: 'Deposit' | 'Withdraw' = 'Deposit';
  customAmount: number | null = null;

  showProfileModal: boolean = false;
  showProfileMenu: boolean = false;
  
  showDeleteModal: boolean = false;
  deletePassword: string = '';

  showToast: boolean = false;
  toastMessage: string = '';

  isUpdating: boolean = false;

  private authSub: any;

  constructor(
    private hotbarService: HotbarService,
    private matchService: MatchService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadUserInfo();
    this.authSub = this.hotbarService.onLoginSuccess.subscribe(() => {
      this.loadUserInfo();
    });
  }

  ngOnDestroy(): void {
    if (this.authSub) this.authSub.unsubscribe();
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

  openTransactionModal(type: 'Deposit' | 'Withdraw') {
    this.transactionType = type;
    this.customAmount = null;
    this.showTransactionModal = true;
    this.showProfileModal = false;
    this.showProfileMenu = false; 
    this.showDeleteModal = false;
  }
  closeTransactionModal() { this.showTransactionModal = false; }

  openProfileModal() {
    this.showProfileModal = true;
    this.showTransactionModal = false;
    this.showProfileMenu = false; 
    this.showDeleteModal = false;
    
    this.isEditingUsername = false;
    this.isEditingEmail = false;
    this.editUsername = this.username;
    this.editEmail = this.email;
  }
  closeProfileModal() { this.showProfileModal = false; }

  openDeleteModal() {
    this.showDeleteModal = true;
    this.showProfileMenu = false;
    this.showTransactionModal = false;
    this.showProfileModal = false;
    this.deletePassword = ''; 
  }
  closeDeleteModal() { this.showDeleteModal = false; }

  toggleProfileMenu() { this.showProfileMenu = !this.showProfileMenu; }

  onLogout() {
    this.showProfileMenu = false; 
    this.hotbarService.logout();  
    window.location.reload();
  }

  saveProfileChanges() {
    if(!this.isEditingUsername && !this.isEditingEmail) {
      this.closeProfileModal(); return;
    }
    if(!this.editUsername || !this.editEmail) {
      alert("Câmpurile nu pot fi goale!"); return;
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
        this.closeProfileModal();
        setTimeout(() => { this.displaySuccessToast(`Profilul a fost actualizat cu succes!`); }, 150);
      },
      error: (err: any) => {
        let errorMsg = 'Eroare de rețea.';
        if (err.status === 409) errorMsg = 'Acest username este deja luat!';
        else if (err.error && typeof err.error === 'string') errorMsg = err.error;
        alert(`Eroare:\n${errorMsg}`);
      }
    });
  }

  confirmDeleteAccount() {
    if (!this.deletePassword) {
      alert('Te rog introdu parola pentru a continua.');
      return;
    }

    this.hotbarService.login({ username: this.username, password: this.deletePassword }).subscribe({
      next: () => {
        this.hotbarService.deleteAccount(this.username).subscribe({
          next: () => {
            this.closeDeleteModal();
            // FĂRĂ ALERT - DOAR LOGOUT SILENȚIOS
            this.onLogout(); 
          },
          error: (err: any) => {
            console.error("EROARE DELETE:", err);
            let msg = err.error?.message || err.message || "Eroare necunoscută";
            alert(`A apărut o eroare la ștergere!\n\nCod Eroare: ${err.status}\nDetalii: ${msg}\n\nDacă ai codul 404, trebuie să dai RESTART la serverul C#!`);
          }
        });
      },
      error: () => {
        alert('Parola introdusă este incorectă!');
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

  confirmTransaction(amount?: number) {
    const finalAmount = amount !== undefined ? amount : this.customAmount;
    if (!finalAmount || finalAmount <= 0) {
      alert('Te rugăm să introduci o sumă validă mai mare de 0.'); return;
    }

    if (this.transactionType === 'Deposit') {
      this.hotbarService.depositFunds(finalAmount).subscribe({
        next: (response: any) => {
          this.updateBalance(response.newBalance || response.NewBalance);
          this.closeTransactionModal(); 
          setTimeout(() => { this.displaySuccessToast(`Depunere de $${finalAmount} realizată cu succes!`); }, 150);
        },
        error: (err: any) => alert('A apărut o eroare la depunere.')
      });
    } else {
      this.hotbarService.withdrawFunds(finalAmount).subscribe({
        next: (response: any) => {
          this.updateBalance(response.newBalance || response.NewBalance);
          this.closeTransactionModal(); 
          setTimeout(() => { this.displaySuccessToast(`Retragere de $${finalAmount} realizată cu succes!`); }, 150);
        },
        error: (err: any) => {
          if (err.status === 400 && err.error) alert(err.error);
          else alert('A apărut o eroare la retragere.');
        }
      });
    }
  }

  private updateBalance(newBal: number) {
    this.balance = newBal;
    const userStr = localStorage.getItem('user');
    if (userStr) {
      let userObj = JSON.parse(userStr);
      userObj.balance = newBal;
      localStorage.setItem('user', JSON.stringify(userObj));
    }
  }

  updateFromApi() {
    this.isUpdating = true;
    this.matchService.updateFromApi().subscribe({
      next: (res) => {
        this.isUpdating = false;
        this.displaySuccessToast(res.message || 'Matches updated successfully!');
        setTimeout(() => { window.location.reload(); }, 1500);
      },
      error: (err) => {
        this.isUpdating = false;
        alert('Eroare la actualizarea meciurilor din API.');
      }
    });
  }
}