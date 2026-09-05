import { TestBed } from '@angular/core/testing';
import { EmployeeParticipationService } from './employee-participation-service';

describe('EmployeeParticipationService', () => {
  let service: EmployeeParticipationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EmployeeParticipationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
