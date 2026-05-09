import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatchService } from '../../services/match_service';
import { Match } from '../../models/match_model';

@Component({
  selector: 'app-matches',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './matches.component.html',
  styleUrl: './matches.component.css'
})
export class MatchesComponent implements OnInit {
  matches: Match[] = [];
  filteredMatches: Match[] = [];
  leagues = ['All', 'SuperLiga', 'Premier League', 'La Liga', 'Serie A', 'Bundesliga', 'Ligue 1'];
  selectedLeague = 'All';
  loading = true;
  error = false;

  constructor(private matchService: MatchService) {}

  ngOnInit(): void {
    this.loadMatches();
  }

  loadMatches(): void {
    this.loading = true;
    this.matchService.getMatches().subscribe({
      next: (data) => {
        this.matches = data;
        this.filteredMatches = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading matches:', err);
        this.error = true;
        this.loading = false;
      }
    });
  }

  filterByLeague(league: string): void {
    this.selectedLeague = league;
    if (league === 'All') {
      this.filteredMatches = this.matches;
    } else {
      this.filteredMatches = this.matches.filter(m => m.league === league);
    }
  }
}