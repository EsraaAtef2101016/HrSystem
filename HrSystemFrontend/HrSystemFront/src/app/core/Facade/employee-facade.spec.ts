import { TestBed } from '@angular/core/testing';
import { EmployeeFacade } from './employee-facade';

describe('EmployeeFacade', () => {
  let service: EmployeeFacade;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EmployeeFacade);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
