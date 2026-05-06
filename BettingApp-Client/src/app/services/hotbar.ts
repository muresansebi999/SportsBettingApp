import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, of } from 'rxjs'; // Am adăugat Subject și of
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class HotbarService {
  private apiUrl = 'http://localhost:5257/api/hotbar';
  private authUrl = 'http://localhost:5257/api/auth';

  // ASTA E NOU: Un "difuzor" care va anunța hotbar-ul să se actualizeze instant
  public onLoginSuccess = new Subject<void>();

  constructor(private http: HttpClient) { }

  private getUsername(): string {
    const userString = localStorage.getItem('user');
    if (userString) {
      const user = JSON.parse(userString);
      return user.username || user.Username || '';
    }
    return '';
  }

  getHotbarInfo(): Observable<any> {
    const username = this.getUsername();
    // Dacă nu găsește un user logat, trimitem zero ca să nu dea eroare în consolă
    if (!username) {
      return of({ username: '', balance: 0 }); 
    }
    return this.http.get(`${this.apiUrl}/info/${username}`);
  }

  depositFunds(amount: number): Observable<any> {
    const username = this.getUsername();
    return this.http.post(`${this.apiUrl}/deposit`, { 
      username: username, 
      amount: amount 
    });
  }

  login(model: any): Observable<any> {
    return this.http.post(`${this.authUrl}/login`, model).pipe(
      map((user: any) => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          // AM ADĂUGAT: Strigăm la Hotbar să se reîncarce după ce am salvat userul!
          this.onLoginSuccess.next();
        }
        return user;
      })
    );
  }

  logout(): void {
    localStorage.removeItem('user');
    this.onLoginSuccess.next(); // Resetăm și când dă logout
  }

  getCurrentUser(): any {
    const userString = localStorage.getItem('user');
    return userString ? JSON.parse(userString) : null;
  }
}