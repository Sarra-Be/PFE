import { TestBed } from '@angular/core/testing';

import { FilePredictionService } from './file-prediction.service';

describe('FilePredictionService', () => {
  let service: FilePredictionService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FilePredictionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
