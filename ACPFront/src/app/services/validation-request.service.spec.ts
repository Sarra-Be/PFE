import { TestBed } from '@angular/core/testing';

import { ValidationRequestService } from './validation-request.service';

describe('ValidationRequestService', () => {
  let service: ValidationRequestService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ValidationRequestService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
