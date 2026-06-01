import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Match } from '../models/match_model';

@Injectable({
  providedIn: 'root'
})
export class MatchService {
  private apiUrl = 'http://localhost:5257/api/matches';

  constructor(private http: HttpClient) {}

  getMatches(): Observable<Match[]> {
    return this.http.get<Match[]>(this.apiUrl);
  }

  getMatchesByLeague(league: string): Observable<Match[]> {
    return this.http.get<Match[]>(`${this.apiUrl}/league/${league}`);
  }

  updateFromApi(): Observable<any> {
    return this.http.post(`${this.apiUrl}/update`, {});
  }

  settleBets(): Observable<any> {
    return this.http.post(`http://localhost:5257/api/bets/settle`, {});
  }
}