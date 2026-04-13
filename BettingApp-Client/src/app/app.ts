import { Component, OnInit, ChangeDetectorRef } from '@angular/core'; 
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [FormsModule, CommonModule, HttpClientModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  model: any = {};
  registerMode = false;
  currentUser: any = null;

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    const userString = localStorage.getItem('user');
    if (userString) {
      this.currentUser = JSON.parse(userString);
    }
  }

  submitForm(form: NgForm) {
    if (!form.valid) return;

    if (this.registerMode) {
      this.register();
    } else {
      this.login();
    }
  }

  login() {
    this.http.post('http://localhost:5257/api/auth/login', this.model).subscribe({
      next: (response: any) => {
        this.currentUser = response;
        localStorage.setItem('user', JSON.stringify(response));
        
        this.cdr.detectChanges();
      },
      error: (err) => alert(err.error)
    });
  }

  register() {
    if (!this.model.dateOfBirth || this.model.dateOfBirth.toString().includes('0001')) {
      alert("Te rugăm să introduci o dată de naștere validă.");
      return; 
    } 

    const namePattern = /^[a-zA-ZĂÂÎȘȚăâîșț\s-]+$/;

    if (!namePattern.test(this.model.firstName) || !namePattern.test(this.model.lastName)) {
      alert("Numele și prenumele trebuie să conțină doar litere!");
      return;
    }

    this.http.post('http://localhost:5257/api/auth/register', this.model).subscribe({
      next: () => {
        alert('Cont creat!');
        this.registerToggle();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.log(err);
        if (err.status === 409) {
          alert('Acest nume de utilizator este deja folosit. Te rugăm să alegi altul!');
        } else if (typeof err.error === 'string') {
          alert(err.error); 
        } else if (err.error && err.error.errors) {
          const validationErrors = err.error.errors;
          let errorMessages = '';
          
          for (const key in validationErrors) {
            if (validationErrors[key]) {
              errorMessages += `${validationErrors[key].join(', ')}\n`;
            }
          }
          alert(errorMessages || 'Date invalide');
        } else {
          alert('Eroare necunoscută la înregistrare');
        }
      }
    });
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser = null;
    this.cdr.detectChanges();
  }

  registerToggle() {
    this.registerMode = !this.registerMode;
    this.model = {};
    this.cdr.detectChanges();
  }
}