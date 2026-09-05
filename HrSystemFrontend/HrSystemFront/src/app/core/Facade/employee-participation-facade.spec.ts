import { TestBed } from '@angular/core/testing';
import { EmployeeParticipationFacade } from './employee-participation-facade';

describe('EmployeeParticipationFacade', () => {
  let service: EmployeeParticipationFacade;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EmployeeParticipationFacade);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
