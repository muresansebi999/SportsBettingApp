import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatchService } from '../../services/match_service';
import { Match } from '../../models/match_model';
import { BetSlipService } from '../../services/bet_slip_service';

@Component({
  selector: 'app-matches',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './matches.component.html',
  styleUrl: './matches.component.css'
})
export class MatchesComponent implements OnInit {
  matches: Match[] = [];
  
  upcomingMatches: Match[] = [];
  liveMatches: Match[] = [];
  finishedMatches: Match[] = [];
  
  tabState: 'upcoming' | 'live' | 'finished' = 'upcoming';

  leagues: string[] = ['All'];
  selectedLeague = 'All';
  loading = true;
  error = false;

  constructor(private matchService: MatchService, private betSlip: BetSlipService, private cdr: ChangeDetectorRef) {

  }

  ngOnInit(): void {
    this.loadMatches();
  }

  loadMatches(): void {
    this.loading = true;
    this.cdr.detectChanges();
    this.matchService.getMatches().subscribe({
      next: (data) => {
        this.matches = data.map(m => ({
          ...m,
          isFinished: !!m.isFinished
        }));
        
        const uniqueLeagues = Array.from(new Set(this.matches.map(m => m.league)));
        this.leagues = ['All', ...uniqueLeagues];
        
        this.applyFilters();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading matches:', err);
        this.error = true;
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  filterByLeague(league: string): void {
    this.selectedLeague = league;
    this.applyFilters();
  }

  setTab(tab: 'upcoming' | 'live' | 'finished'): void {
    this.tabState = tab;
  }

  applyFilters(): void {
    let base = this.matches;
    if (this.selectedLeague !== 'All') {
      base = base.filter(m => m.league === this.selectedLeague);
    }
    
    const now = new Date().getTime();
    
    this.upcomingMatches = base
      .filter(m => !m.isFinished && new Date(m.startTime).getTime() > now)
      .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime());
      
    this.liveMatches = base
      .filter(m => !m.isFinished && new Date(m.startTime).getTime() <= now)
      .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime());
      
    this.finishedMatches = base
      .filter(m => m.isFinished)
      .sort((a, b) => new Date(b.startTime).getTime() - new Date(a.startTime).getTime());
  }

  addToSlip(match: Match, outcome: '1' | 'X' | '2'): void {
  const odd =
    outcome === '1' ? match.homeOdds :
    outcome === '2' ? match.awayOdds :
    match.drawOdds;

  // Don't add a selection with no/zero odd.
  if (!odd) return;

  this.betSlip.addSelection({
    matchId: match.id,
    homeTeam: match.homeTeam,
    awayTeam: match.awayTeam,
    outcome: outcome,
    odd: odd
  });
}

isSelected(match: Match, outcome: '1' | 'X' | '2'): boolean {
  return this.betSlip.getSelectedOutcome(match.id) === outcome;
}

}