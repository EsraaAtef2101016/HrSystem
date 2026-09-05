import { TestBed } from '@angular/core/testing';
import { LeaveReviewFacade } from './leave-review-facade';

describe('LeaveReviewFacade', () => {
  let service: LeaveReviewFacade;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LeaveReviewFacade);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
