import { TestBed } from '@angular/core/testing';
import { LeavePolicyService } from './leave-policy-service';

describe('LeavePolicyService', () => {
  let service: LeavePolicyService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LeavePolicyService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
