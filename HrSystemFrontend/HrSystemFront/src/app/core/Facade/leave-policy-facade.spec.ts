import { TestBed } from '@angular/core/testing';
import { LeavePolicyFacade } from './leave-policy-facade';

describe('LeavePolicyFacade', () => {
  let service: LeavePolicyFacade;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LeavePolicyFacade);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
