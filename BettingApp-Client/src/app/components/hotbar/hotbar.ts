import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core'; 
import { CommonModule } from '@angular/common'; 
import { HotbarService } from '../../services/hotbar';  

@Component({
  selector: 'app-hotbar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hotbar.html', 
  styleUrls: ['./hotbar.css']   
})
export class HotbarComponent implements OnInit, OnDestroy {
  username: string = '';
  balance: number = 0;
  showDepositModal: boolean = false; 
  
  // Variabile pentru bara custom de succes
  showToast: boolean = false;
  toastMessage: string = '';

  private checkInterval: any; 
  private lastKnownUser: string = '';

  // Am adăugat ChangeDetectorRef pentru a ne asigura că Angular actualizează interfața la timp
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
        }
        // Forțăm interfața să se actualizeze
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
        this.cdr.detectChanges();
      },
      error: (err: any) => console.error('Eroare la încărcarea datelor:', err)
    });
  }

  openModal() {
    this.showDepositModal = true;
  }

  closeModal() {
    this.showDepositModal = false;
  }

  displaySuccessToast(message: string) {
    this.toastMessage = message;
    this.showToast = true;
    this.cdr.detectChanges(); // Spunem lui Angular să arate bara imediat
    
    // După fix 2 secunde, o ascundem
    setTimeout(() => {
      this.showToast = false;
      this.cdr.detectChanges(); // Spunem lui Angular să o ascundă
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

        // 1. Închidem imediat meniul negru
        this.closeModal(); 
        
        // 2. Așteptăm o fracțiune de secundă (150ms) ca să se închidă meniul frumos, apoi afișăm bara albă
        setTimeout(() => {
          this.displaySuccessToast(`Depunere de $${amount} realizată cu succes!`);
        }, 150);

      },
      error: (err: any) => {
        console.error('Eroare la depunere', err);
        alert('A apărut o eroare la depunere.');
      }
    });
  }
}