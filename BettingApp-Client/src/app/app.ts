import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, FormsModule, CommonModule, HttpClientModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  title = signal('BettingApp-Client');
  model: any = {}; // Obiectul care colectează datele din formular

  constructor(private http: HttpClient) {}

  login() {
    console.log('Încercare login pentru:', this.model.username);
    
    // Verifică portul: 5257 (sau portul tău de .NET)
    this.http.post('http://localhost:5257/api/auth/login', this.model).subscribe({
      next: (response) => {
        console.log('Succes Backend:', response);
        alert('Te-ai logat cu succes, ' + this.model.username + '!');
      },
      error: (error) => {
        console.error('Eroare Backend:', error);
        alert('Eroare: ' + (error.error || 'Nu m-am putut conecta la server'));
      }
    });
  }
}