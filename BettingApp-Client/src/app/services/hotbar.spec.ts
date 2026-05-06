import { TestBed } from '@angular/core/testing';
import { HotbarService } from './hotbar';

describe('HotbarService', () => {
  let service: HotbarService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(HotbarService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});