import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { HotbarService } from '../../services/hotbar';  

@Component({
  selector: 'app-hotbar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hotbar.html', 
  styleUrls: ['./hotbar.css']   
})
export class HotbarComponent implements OnInit {
  username: string = '';
  balance: number = 0;
  
  // Această variabilă controlează dacă meniul e vizibil sau nu
  showDepositModal: boolean = false; 

  constructor(private hotbarService: HotbarService) {}

  ngOnInit(): void {
    this.loadUserInfo();
  }

  loadUserInfo() {
    this.hotbarService.getHotbarInfo().subscribe({
      next: (data: any) => {
        this.username = data.username;
        this.balance = data.balance;
      },
      error: (err: any) => console.error('Eroare la încărcarea datelor.', err)
    });
  }

  // Funcții pentru meniul nostru personalizat
  openModal() {
    this.showDepositModal = true;
  }

  closeModal() {
    this.showDepositModal = false;
  }

  // Se apelează când apeși pe un buton cu sumă (ex: $100)
  confirmDeposit(amount: number) {
    this.hotbarService.depositFunds(amount).subscribe({
      next: (response: any) => {
        // Succes! Actualizăm banii și închidem fereastra
        this.balance = response.newBalance; 
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