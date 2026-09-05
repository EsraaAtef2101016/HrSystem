import { TestBed } from '@angular/core/testing';
import { LeaveReviewService } from './leave-review-service';

describe('LeaveReviewService', () => {
  let service: LeaveReviewService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LeaveReviewService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
