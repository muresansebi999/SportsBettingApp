import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class HotbarService {
  private apiUrl = 'http://localhost:5257/api/hotbar';

  constructor(private http: HttpClient) { }

  // O funcție simplă care scoate doar Numele din memoria browserului
  private getUsername(): string {
    const userString = localStorage.getItem('user');
    if (userString) {
      const user = JSON.parse(userString);
      return user.username; // Returnează ex: "noris"
    }
    return '';
  }

  getHotbarInfo(): Observable<any> {
    const username = this.getUsername();
    // Trimitem numele direct prin link-ul către C#
    return this.http.get(`${this.apiUrl}/info/${username}`);
  }

  depositFunds(amount: number): Observable<any> {
    const username = this.getUsername();
    // Trimitem atât numele, cât și suma pe care vrem să o depunem
    return this.http.post(`${this.apiUrl}/deposit`, { 
      username: username, 
      amount: amount 
    });
  }
}