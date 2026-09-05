import { TestBed } from '@angular/core/testing';
import { LeaveRequestFacade } from './leave-request-facade';

describe('LeaveRequestFacade', () => {
  let service: LeaveRequestFacade;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LeaveRequestFacade);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
