import { Component, OnInit, OnDestroy } from '@angular/core'; 
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
  private checkInterval: any; 

  constructor(private hotbarService: HotbarService) {}

  ngOnInit(): void {
    // Hotbar-ul se uită doar în memorie de 2 ori pe secundă.
    // În secunda în care te-ai logat, datele sunt deja acolo grație ideii tale!
    this.checkInterval = setInterval(() => {
      const currentUser = this.hotbarService.getCurrentUser();
      
      if (currentUser) {
        this.username = currentUser.username || currentUser.Username || 'U';
        this.balance = currentUser.balance || currentUser.Balance || 0;
      } else {
        this.username = 'U';
        this.balance = 0;
      }
    }, 500);
  }

  ngOnDestroy(): void {
    if (this.checkInterval) {
      clearInterval(this.checkInterval);
    }
  }

  openModal() {
    this.showDepositModal = true;
  }

  closeModal() {
    this.showDepositModal = false;
  }

  confirmDeposit(amount: number) {
    this.hotbarService.depositFunds(amount).subscribe({
      next: (response: any) => {
        const newBal = response.newBalance || response.NewBalance;
        
        // Când depunem, actualizăm direct memoria browser-ului
        // astfel încât "radarul" nostru să vadă noii bani instant!
        const userStr = localStorage.getItem('user');
        if (userStr) {
          let userObj = JSON.parse(userStr);
          userObj.balance = newBal;
          localStorage.setItem('user', JSON.stringify(userObj));
        }

        this.closeModal(); 
        alert(`Ai depus cu succes $${amount}!`);
      },
      error: (err: any) => {
        console.error('Eroare la depunere', err);
        alert('A apărut o eroare la depunere.');
      }
    });
  }
}